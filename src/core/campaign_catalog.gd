class_name EngineeringCampaignCatalog
extends RefCounted

var campaign_id: String = ""
var title: String = ""
var chapters: Array[Dictionary] = []
var challenges: Array[EngineeringChallengeDefinition] = []
var challenge_meta: Array[Dictionary] = []

func load_json(path: String) -> PackedStringArray:
	chapters.clear()
	challenges.clear()
	challenge_meta.clear()
	var errors := PackedStringArray()
	if not FileAccess.file_exists(path):
		errors.append("Campaign file not found: %s" % path)
		return errors
	var file := FileAccess.open(path, FileAccess.READ)
	var parsed = JSON.parse_string(file.get_as_text())
	if typeof(parsed) != TYPE_DICTIONARY:
		errors.append("Invalid campaign JSON: %s" % path)
		return errors
	if int(parsed.get("schema_version", 0)) != 1:
		errors.append("Unsupported campaign schema version")
		return errors
	campaign_id = str(parsed.get("campaign_id", ""))
	title = str(parsed.get("title", ""))
	var source_chapters: Array = parsed.get("chapters", [])
	for chapter_data in source_chapters:
		if typeof(chapter_data) != TYPE_DICTIONARY:
			continue
		var chapter := {
			"chapter_id": str(chapter_data.get("chapter_id", "")),
			"chapter_number": int(chapter_data.get("chapter_number", chapters.size() + 1)),
			"title": str(chapter_data.get("title", "")),
			"unlock_stars": int(chapter_data.get("unlock_stars", 0)),
			"start_index": challenges.size(),
			"count": 0,
		}
		var source_challenges: Array = chapter_data.get("challenges", [])
		for challenge_data in source_challenges:
			if typeof(challenge_data) != TYPE_DICTIONARY:
				continue
			var challenge := EngineeringChallengeDefinition.from_dictionary(challenge_data)
			var challenge_errors := challenge.validate()
			if not challenge_errors.is_empty():
				for error in challenge_errors:
					errors.append("%s: %s" % [String(challenge.challenge_id), error])
				continue
			challenges.append(challenge)
			challenge_meta.append({
				"chapter_id": chapter["chapter_id"],
				"chapter_number": chapter["chapter_number"],
				"chapter_title": chapter["title"],
				"unlock_stars": chapter["unlock_stars"],
				"level_number": int(challenge_data.get("campaign", {}).get("level_number", challenges.size())),
			})
			chapter["count"] = int(chapter["count"]) + 1
		chapters.append(chapter)
	if challenges.is_empty():
		errors.append("Campaign contains no valid challenges")
	return errors

func size() -> int:
	return challenges.size()

func get_challenge(index: int) -> EngineeringChallengeDefinition:
	if index < 0 or index >= challenges.size():
		return null
	return challenges[index]

func get_meta(index: int) -> Dictionary:
	if index < 0 or index >= challenge_meta.size():
		return {}
	return challenge_meta[index]

func is_unlocked(index: int, total_stars: int) -> bool:
	var meta := get_meta(index)
	if meta.is_empty():
		return false
	return total_stars >= int(meta.get("unlock_stars", 0))

func next_unlocked_index(current_index: int, total_stars: int) -> int:
	for index in range(current_index + 1, challenges.size()):
		if is_unlocked(index, total_stars):
			return index
	return current_index

func previous_index(current_index: int) -> int:
	return max(0, current_index - 1)
