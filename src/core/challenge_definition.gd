class_name EngineeringChallengeDefinition
extends Resource

@export var schema_version: int = 1
@export var challenge_id: StringName
@export var playground_id: StringName
@export var title: String = ""
@export_multiline var description: String = ""
@export var difficulty: int = 1
@export var presentation_mode: String = "explorer"
@export var starting_state: Dictionary = {}
@export var allowed_tools: PackedStringArray = PackedStringArray()
@export var constraints: Dictionary = {}
@export var success_conditions: Dictionary = {}
@export var concept_unlocks: PackedStringArray = PackedStringArray()
@export var hints: PackedStringArray = PackedStringArray()
@export var rewards: Dictionary = {}
@export var scoring_weights: Dictionary = {}
@export var domain_config: Dictionary = {}

static func from_dictionary(data: Dictionary) -> EngineeringChallengeDefinition:
	var challenge := EngineeringChallengeDefinition.new()
	challenge.schema_version = int(data.get("schema_version", 1))
	challenge.challenge_id = StringName(data.get("challenge_id", ""))
	challenge.playground_id = StringName(data.get("playground_id", ""))
	challenge.title = str(data.get("title", ""))
	challenge.description = str(data.get("description", ""))
	challenge.difficulty = int(data.get("difficulty", 1))
	challenge.presentation_mode = str(data.get("presentation_mode", "explorer"))
	challenge.starting_state = data.get("starting_state", {})
	challenge.allowed_tools = PackedStringArray(data.get("allowed_tools", []))
	challenge.constraints = data.get("constraints", {})
	challenge.success_conditions = data.get("success_conditions", {})
	challenge.concept_unlocks = PackedStringArray(data.get("concept_unlocks", []))
	challenge.hints = PackedStringArray(data.get("hints", []))
	challenge.rewards = data.get("rewards", {})
	challenge.scoring_weights = data.get("scoring_weights", {})
	challenge.domain_config = data.get("domain_config", {})
	return challenge

func validate() -> PackedStringArray:
	var errors := PackedStringArray()
	if schema_version != 1:
		errors.append("unsupported schema_version: %d" % schema_version)
	if challenge_id == &"":
		errors.append("challenge_id is required")
	if playground_id == &"":
		errors.append("playground_id is required")
	if title.strip_edges().is_empty():
		errors.append("title is required")
	if difficulty < 1:
		errors.append("difficulty must be >= 1")
	if success_conditions.is_empty():
		errors.append("success_conditions are required")
	if scoring_weights.is_empty():
		errors.append("scoring_weights are required")
	var total_weight := 0.0
	for value in scoring_weights.values():
		total_weight += float(value)
	if total_weight <= 0.0:
		errors.append("scoring_weights must sum to > 0")
	return errors
