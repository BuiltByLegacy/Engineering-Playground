class_name FlowChallengeEvaluator
extends RefCounted

static func evaluate(challenge: EngineeringChallengeDefinition, context: Dictionary) -> Dictionary:
	var metrics: Dictionary = context.get("metrics", {})
	var weights := challenge.scoring_weights
	var targets := challenge.domain_config.get("targets", {})
	var constraints := challenge.constraints

	var outlet_speed := float(metrics.get("mean_outlet_speed", 0.0))
	var pressure_loss := float(metrics.get("pressure_loss_proxy", 1.0))
	var swirl := float(metrics.get("mean_vorticity_proxy", 1.0))
	var solid_cells := float(metrics.get("solid_cells", 0.0))
	var editable_solid_cells := max(0.0, solid_cells - float(metrics.get("base_solid_cells", 0.0)))

	var target_flow := max(0.0001, float(targets.get("outlet_speed", 0.05)))
	var max_pressure_loss := max(0.0001, float(targets.get("max_pressure_loss", 0.03)))
	var max_swirl := max(0.0001, float(targets.get("max_vorticity", 0.03)))
	var material_budget := max(1.0, float(constraints.get("max_added_solid_cells", 300.0)))

	var dimension_scores := {
		"flow": clamp(outlet_speed / target_flow, 0.0, 1.0) * 100.0,
		"pressure": clamp(1.0 - pressure_loss / max_pressure_loss, 0.0, 1.0) * 100.0,
		"turbulence": clamp(1.0 - swirl / max_swirl, 0.0, 1.0) * 100.0,
		"material": clamp(1.0 - editable_solid_cells / material_budget, 0.0, 1.0) * 100.0,
		"balance": 100.0,
		"cost": 100.0,
		"complexity": 100.0,
		"packaging": 100.0
	}

	var total_weight := 0.0
	var weighted := 0.0
	for key in weights.keys():
		var w := float(weights[key])
		total_weight += w
		weighted += float(dimension_scores.get(key, 0.0)) * w
	var score := 0.0 if total_weight <= 0.0 else weighted / total_weight

	var success := true
	if challenge.success_conditions.has("min_outlet_speed"):
		success = success and outlet_speed >= float(challenge.success_conditions["min_outlet_speed"])
	if challenge.success_conditions.has("max_pressure_loss"):
		success = success and pressure_loss <= float(challenge.success_conditions["max_pressure_loss"])
	if challenge.success_conditions.has("max_vorticity"):
		success = success and swirl <= float(challenge.success_conditions["max_vorticity"])
	if challenge.success_conditions.has("minimum_score"):
		success = success and score >= float(challenge.success_conditions["minimum_score"])

	var feedback := _build_feedback(dimension_scores, outlet_speed, pressure_loss, swirl, challenge.presentation_mode)
	return {
		"success": success,
		"score": snapped(score, 0.1),
		"dimensions": dimension_scores,
		"metrics": metrics,
		"feedback": feedback,
		"grade": _grade(score)
	}

static func _build_feedback(scores: Dictionary, outlet_speed: float, pressure_loss: float, swirl: float, mode: String) -> PackedStringArray:
	var feedback := PackedStringArray()
	if mode == "engineer":
		feedback.append("Outlet velocity proxy: %.4f" % outlet_speed)
		feedback.append("Pressure-loss proxy: %.4f" % pressure_loss)
		feedback.append("Mean swirl proxy: %.4f" % swirl)
	else:
		feedback.append("Flow: %s" % _quality(float(scores["flow"])))
		feedback.append("Pressure: %s" % _quality(float(scores["pressure"])))
		feedback.append("Swirls: %s" % _inverse_quality(float(scores["turbulence"])))

	var weakest := "flow"
	for key in ["pressure", "turbulence", "material"]:
		if float(scores[key]) < float(scores[weakest]):
			weakest = key
	match weakest:
		"flow": feedback.append("Try opening the flow path or smoothing a restriction.")
		"pressure": feedback.append("Pressure is being lost. Look for sharp constrictions and abrupt turns.")
		"turbulence": feedback.append("The flow is recirculating. Smooth transitions and reduce sudden direction changes.")
		"material": feedback.append("The design works, but it uses more material than necessary.")
	return feedback

static func _quality(score: float) -> String:
	if score >= 85.0: return "Great"
	if score >= 65.0: return "Good"
	if score >= 40.0: return "Needs work"
	return "Low"

static func _inverse_quality(score: float) -> String:
	if score >= 85.0: return "Low"
	if score >= 65.0: return "Moderate"
	return "High"

static func _grade(score: float) -> String:
	if score >= 90.0: return "S"
	if score >= 80.0: return "A"
	if score >= 70.0: return "B"
	if score >= 60.0: return "C"
	return "D"
