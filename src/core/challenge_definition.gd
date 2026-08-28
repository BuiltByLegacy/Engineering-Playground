class_name EngineeringChallengeDefinition
extends Resource

@export var challenge_id: StringName
@export var playground_id: StringName
@export var title: String = ""
@export_multiline var description: String = ""
@export var difficulty: int = 1
@export var allowed_tools: PackedStringArray = PackedStringArray()
@export var concept_unlocks: PackedStringArray = PackedStringArray()
@export var scoring_weights: Dictionary = {}
@export var domain_config: Dictionary = {}

func validate() -> PackedStringArray:
	var errors := PackedStringArray()
	if challenge_id == &"":
		errors.append("challenge_id is required")
	if playground_id == &"":
		errors.append("playground_id is required")
	if title.strip_edges().is_empty():
		errors.append("title is required")
	return errors
