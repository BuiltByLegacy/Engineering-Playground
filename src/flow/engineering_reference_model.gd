class_name FlowEngineeringReferenceModel
extends RefCounted

# Lightweight engineering-reference calculations for gameplay displays.
# These formulas are classical approximations and must not be presented as
# validated CFD or design-certification results.

const G := 9.80665

static func volumetric_flow(area_m2: float, velocity_mps: float) -> float:
	return max(area_m2, 0.0) * max(velocity_mps, 0.0)

static func mass_flow(density_kg_m3: float, area_m2: float, velocity_mps: float) -> float:
	return max(density_kg_m3, 0.0) * volumetric_flow(area_m2, velocity_mps)

static func dynamic_pressure(density_kg_m3: float, velocity_mps: float) -> float:
	return 0.5 * max(density_kg_m3, 0.0) * velocity_mps * velocity_mps

static func reynolds_number(density_kg_m3: float, velocity_mps: float, hydraulic_diameter_m: float, dynamic_viscosity_pa_s: float) -> float:
	if dynamic_viscosity_pa_s <= 0.0:
		return 0.0
	return max(density_kg_m3, 0.0) * abs(velocity_mps) * max(hydraulic_diameter_m, 0.0) / dynamic_viscosity_pa_s

static func darcy_friction_factor(reynolds: float, relative_roughness: float = 0.0) -> float:
	if reynolds <= 0.0:
		return 0.0
	if reynolds < 2300.0:
		return 64.0 / reynolds
	# Haaland explicit approximation to the Colebrook relation.
	var rr := max(relative_roughness, 0.0)
	var term := pow(rr / 3.7, 1.11) + 6.9 / reynolds
	if term <= 0.0:
		return 0.0
	var denominator := -1.8 * log(term) / log(10.0)
	if is_zero_approx(denominator):
		return 0.0
	return 1.0 / (denominator * denominator)

static func darcy_weisbach_pressure_loss(friction_factor: float, length_m: float, hydraulic_diameter_m: float, density_kg_m3: float, velocity_mps: float) -> float:
	if hydraulic_diameter_m <= 0.0:
		return 0.0
	return max(friction_factor, 0.0) * max(length_m, 0.0) / hydraulic_diameter_m * dynamic_pressure(density_kg_m3, velocity_mps)

static func minor_loss_pressure(loss_coefficient_k: float, density_kg_m3: float, velocity_mps: float) -> float:
	return max(loss_coefficient_k, 0.0) * dynamic_pressure(density_kg_m3, velocity_mps)

static func total_reference_pressure_loss(reference: Dictionary) -> Dictionary:
	var rho := float(reference.get("density_kg_m3", 1.225))
	var mu := float(reference.get("dynamic_viscosity_pa_s", 0.0000181))
	var velocity := float(reference.get("reference_velocity_mps", 1.0))
	var diameter := float(reference.get("hydraulic_diameter_m", 0.05))
	var length := float(reference.get("length_m", 1.0))
	var roughness := float(reference.get("roughness_m", 0.0))
	var k_total := float(reference.get("minor_loss_k", 0.0))
	var area := float(reference.get("area_m2", PI * diameter * diameter * 0.25))
	var re := reynolds_number(rho, velocity, diameter, mu)
	var rel_roughness := roughness / diameter if diameter > 0.0 else 0.0
	var f := darcy_friction_factor(re, rel_roughness)
	var major := darcy_weisbach_pressure_loss(f, length, diameter, rho, velocity)
	var minor := minor_loss_pressure(k_total, rho, velocity)
	return {
		"flow_m3_s": volumetric_flow(area, velocity),
		"mass_flow_kg_s": mass_flow(rho, area, velocity),
		"reynolds": re,
		"friction_factor": f,
		"dynamic_pressure_pa": dynamic_pressure(rho, velocity),
		"major_loss_pa": major,
		"minor_loss_pa": minor,
		"total_loss_pa": major + minor,
		"flow_regime": flow_regime(re)
	}

static func flow_regime(reynolds: float) -> String:
	if reynolds < 2300.0:
		return "Laminar"
	if reynolds < 4000.0:
		return "Transitional"
	return "Turbulent"

static func default_reference_for_theme(theme: String) -> Dictionary:
	match theme:
		"residential_plumbing":
			return {"fluid":"water", "density_kg_m3":998.2, "dynamic_viscosity_pa_s":0.001002, "hydraulic_diameter_m":0.019, "length_m":9.0, "reference_velocity_mps":1.5, "roughness_m":0.0000015, "minor_loss_k":3.0}
		"automotive_exhaust":
			return {"fluid":"air-like gas", "density_kg_m3":1.10, "dynamic_viscosity_pa_s":0.000019, "hydraulic_diameter_m":0.0635, "length_m":3.2, "reference_velocity_mps":18.0, "roughness_m":0.000045, "minor_loss_k":4.0}
		"hvac_distribution":
			return {"fluid":"air", "density_kg_m3":1.204, "dynamic_viscosity_pa_s":0.0000181, "hydraulic_diameter_m":0.20, "length_m":8.0, "reference_velocity_mps":4.0, "roughness_m":0.00015, "minor_loss_k":2.5}
		"manifold_optimization":
			return {"fluid":"water", "density_kg_m3":998.2, "dynamic_viscosity_pa_s":0.001002, "hydraulic_diameter_m":0.025, "length_m":1.2, "reference_velocity_mps":2.0, "roughness_m":0.0000015, "minor_loss_k":5.0}
		_:
			return {"fluid":"air", "density_kg_m3":1.204, "dynamic_viscosity_pa_s":0.0000181, "hydraulic_diameter_m":0.10, "length_m":1.0, "reference_velocity_mps":2.0, "roughness_m":0.00001, "minor_loss_k":1.0}
