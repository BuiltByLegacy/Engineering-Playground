class_name FlowPlayground
extends EngineeringPlaygroundModule

func get_id() -> StringName:
	return &"flow"

func get_display_name() -> String:
	return "Flow Lab"

func get_description() -> String:
	return "Design, test, and improve fluid systems through interactive 2D simulation."

func create_simulation_adapter() -> EngineeringSimulationAdapter:
	var solver := FlowLbmSolver.new()
	solver.configure({
		"width": 96,
		"height": 54,
		"inlet_velocity": 0.065,
		"steps_per_tick": 2
	})
	return solver

func get_capabilities() -> PackedStringArray:
	return PackedStringArray([
		"2d_flow",
		"velocity_field",
		"density_field",
		"solid_boundaries"
	])
