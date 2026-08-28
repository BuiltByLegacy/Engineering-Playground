class_name EngineeringChallengeEngine
extends RefCounted

signal challenge_started(challenge_id: StringName)
signal attempt_recorded(challenge_id: StringName, attempt: int)
signal challenge_evaluated(challenge_id: StringName, result: Dictionary)

const SAVE_PATH := "user://challenge_progress.cfg"

var current: EngineeringChallengeDefinition
var attempts := 0
var _started_at_msec := 0
var _first_run_at_msec := 0
var _progress := ConfigFile.new()

func _init() -> void:
	_progress.load(SAVE_PATH)

func load_json(path: String) -> EngineeringChallengeDefinition:
	if not FileAccess.file_exists(path):
		push_error("Challenge file not found: %s" % path)
		return null
	var file := FileAccess.open(path, FileAccess.READ)
	var parsed = JSON.parse_string(file.get_as_text())
	if typeof(parsed) != TYPE_DICTIONARY:
		push_error("Invalid challenge JSON: %s" % path)
		return null
	return EngineeringChallengeDefinition.from_dictionary(parsed)

func start_challenge(challenge: EngineeringChallengeDefinition) -> PackedStringArray:
	var errors := challenge.validate()
	if not errors.is_empty():
		return errors
	current = challenge
	attempts = 0
	_started_at_msec = Time.get_ticks_msec()
	_first_run_at_msec = 0
	challenge_started.emit(current.challenge_id)
	return PackedStringArray()

func begin_attempt() -> void:
	if current == null:
		return
	attempts += 1
	if _first_run_at_msec == 0:
		_first_run_at_msec = Time.get_ticks_msec()
	attempt_recorded.emit(current.challenge_id, attempts)

func evaluate(evaluator: Callable, context: Dictionary) -> Dictionary:
	if current == null:
		return {"success": false, "score": 0, "error": "No active challenge"}
	var result: Dictionary = evaluator.call(current, context)
	result["attempt"] = attempts
	result["challenge_id"] = String(current.challenge_id)
	var previous_best := get_best_score(current.challenge_id)
	result["previous_best"] = previous_best
	result["improvement"] = float(result.get("score", 0.0)) - previous_best
	if bool(result.get("success", false)) and float(result.get("score", 0.0)) > previous_best:
		_save_best(current.challenge_id, float(result["score"]))
	result["best_score"] = get_best_score(current.challenge_id)
	challenge_evaluated.emit(current.challenge_id, result)
	return result

func get_best_score(challenge_id: StringName) -> float:
	return float(_progress.get_value(String(challenge_id), "best_score", 0.0))

func get_time_to_first_run_seconds() -> float:
	if _first_run_at_msec == 0 or _started_at_msec == 0:
		return -1.0
	return float(_first_run_at_msec - _started_at_msec) / 1000.0

func _save_best(challenge_id: StringName, score: float) -> void:
	_progress.set_value(String(challenge_id), "best_score", score)
	_progress.save(SAVE_PATH)
