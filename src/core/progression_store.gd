class_name EngineeringProgressionStore
extends RefCounted

const SAVE_PATH := "user://player_progress.cfg"

var _data := ConfigFile.new()

func _init() -> void:
	_data.load(SAVE_PATH)

func record_result(challenge: EngineeringChallengeDefinition, result: Dictionary) -> Dictionary:
	if challenge == null:
		return {}
	var id := String(challenge.challenge_id)
	var score := float(result.get("score", 0.0))
	var success := bool(result.get("success", false))
	var previous_score := float(_data.get_value(id, "best_score", 0.0))
	var previous_stars := int(_data.get_value(id, "stars", 0))
	var stars := _stars_for_challenge(challenge, score) if success else previous_stars
	if success:
		_data.set_value(id, "completed", true)
		_data.set_value(id, "best_score", max(previous_score, score))
		_data.set_value(id, "best_grade", str(result.get("grade", "")))
		_data.set_value(id, "stars", max(previous_stars, stars))
		for concept in challenge.concept_unlocks:
			unlock_concept(String(concept))
	_data.save(SAVE_PATH)
	return get_challenge_progress(challenge.challenge_id)

func get_challenge_progress(challenge_id: StringName) -> Dictionary:
	var id := String(challenge_id)
	return {
		"completed": bool(_data.get_value(id, "completed", false)),
		"best_score": float(_data.get_value(id, "best_score", 0.0)),
		"best_grade": str(_data.get_value(id, "best_grade", "")),
		"stars": int(_data.get_value(id, "stars", 0))
	}

func unlock_concept(concept_id: String) -> void:
	_data.set_value("concepts", concept_id, true)

func is_concept_unlocked(concept_id: String) -> bool:
	return bool(_data.get_value("concepts", concept_id, false))

func get_unlocked_concepts() -> PackedStringArray:
	var unlocked := PackedStringArray()
	if not _data.has_section("concepts"):
		return unlocked
	for key in _data.get_section_keys("concepts"):
		if bool(_data.get_value("concepts", key, false)):
			unlocked.append(key)
	return unlocked

func set_presentation_mode(mode: String) -> void:
	var normalized := "engineer" if mode.to_lower() == "engineer" else "explorer"
	_data.set_value("preferences", "presentation_mode", normalized)
	_data.save(SAVE_PATH)

func get_presentation_mode() -> String:
	return str(_data.get_value("preferences", "presentation_mode", "explorer"))

func total_stars() -> int:
	var total := 0
	for section in _data.get_sections():
		if section == "concepts" or section == "preferences":
			continue
		total += int(_data.get_value(section, "stars", 0))
	return total

func _stars_for_challenge(challenge: EngineeringChallengeDefinition, score: float) -> int:
	var target_scores: Array = challenge.rewards.get("target_scores", [])
	if target_scores.size() >= 3:
		if score >= float(target_scores[2]):
			return 3
		if score >= float(target_scores[1]):
			return 2
		if score >= float(target_scores[0]):
			return 1
		return 0
	return _stars_for_score(score)

func _stars_for_score(score: float) -> int:
	if score >= 90.0:
		return 3
	if score >= 75.0:
		return 2
	if score >= 60.0:
		return 1
	return 0
