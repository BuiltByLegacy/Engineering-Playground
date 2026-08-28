class_name EngineeringRunTelemetry
extends RefCounted

const TELEMETRY_PATH := "user://run_telemetry.jsonl"

func record(event_name: String, properties: Dictionary = {}) -> void:
	var event := {
		"event": event_name,
		"timestamp_unix": Time.get_unix_time_from_system(),
		"properties": properties
	}
	var file := FileAccess.open(TELEMETRY_PATH, FileAccess.READ_WRITE)
	if file == null:
		file = FileAccess.open(TELEMETRY_PATH, FileAccess.WRITE)
	else:
		file.seek_end()
	file.store_line(JSON.stringify(event))

func challenge_started(challenge: EngineeringChallengeDefinition) -> void:
	record("challenge_started", {
		"challenge_id": String(challenge.challenge_id),
		"difficulty": challenge.difficulty,
		"presentation_mode": challenge.presentation_mode
	})

func attempt_scored(challenge: EngineeringChallengeDefinition, result: Dictionary, time_to_first_run: float) -> void:
	record("attempt_scored", {
		"challenge_id": String(challenge.challenge_id),
		"attempt": int(result.get("attempt", 0)),
		"score": float(result.get("score", 0.0)),
		"success": bool(result.get("success", false)),
		"improvement": float(result.get("improvement", 0.0)),
		"time_to_first_run_seconds": time_to_first_run
	})

func visualization_changed(challenge_id: StringName, mode_name: String) -> void:
	record("visualization_changed", {
		"challenge_id": String(challenge_id),
		"mode": mode_name
	})

func hint_used(challenge_id: StringName, hint_index: int) -> void:
	record("hint_used", {
		"challenge_id": String(challenge_id),
		"hint_index": hint_index
	})
