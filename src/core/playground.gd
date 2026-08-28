class_name EngineeringPlaygroundModule
extends RefCounted

## Domain-neutral contract implemented by every engineering playground.

func get_id() -> StringName:
	return &"unknown"

func get_display_name() -> String:
	return "Unknown Playground"

func get_description() -> String:
	return ""

func create_simulation_adapter() -> EngineeringSimulationAdapter:
	push_error("create_simulation_adapter() must be implemented by playground module")
	return null

func get_capabilities() -> PackedStringArray:
	return PackedStringArray()
