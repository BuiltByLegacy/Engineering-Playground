class_name EngineeringSimulationAdapter
extends RefCounted

## Domain-neutral simulation lifecycle used by the game core.

func configure(_config: Dictionary) -> void:
	pass

func reset() -> void:
	pass

func step(_delta: float) -> void:
	pass

func get_metrics() -> Dictionary:
	return {}

func get_visualization_data() -> Dictionary:
	return {}

func is_ready() -> bool:
	return true
