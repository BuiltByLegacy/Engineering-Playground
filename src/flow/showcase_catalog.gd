class_name FlowShowcaseCatalog
extends RefCounted

var _entries: Array[Dictionary] = []

func load_json(path: String = "res://content/flow/showcases.json") -> PackedStringArray:
	var errors := PackedStringArray()
	_entries.clear()
	if not FileAccess.file_exists(path):
		errors.append("showcase catalog not found: %s" % path)
		return errors
	var file := FileAccess.open(path, FileAccess.READ)
	var parsed = JSON.parse_string(file.get_as_text())
	if typeof(parsed) != TYPE_DICTIONARY:
		errors.append("invalid showcase catalog JSON")
		return errors
	if int(parsed.get("schema_version", 0)) != 1:
		errors.append("unsupported showcase schema_version")
		return errors
	var raw_entries: Array = parsed.get("showcases", [])
	for raw in raw_entries:
		if typeof(raw) != TYPE_DICTIONARY:
			continue
		var id := str(raw.get("id", ""))
		var title := str(raw.get("title", ""))
		var challenge_path := str(raw.get("path", ""))
		if id.is_empty() or title.is_empty() or challenge_path.is_empty():
			errors.append("showcase entry is missing id/title/path")
			continue
		_entries.append(raw)
	return errors

func size() -> int:
	return _entries.size()

func get_entry(index: int) -> Dictionary:
	if index < 0 or index >= _entries.size():
		return {}
	return _entries[index]

func load_challenge(index: int, engine: EngineeringChallengeEngine) -> EngineeringChallengeDefinition:
	var entry := get_entry(index)
	if entry.is_empty() or engine == null:
		return null
	return engine.load_json(str(entry.get("path", "")))

func get_media_metadata(index: int, engine: EngineeringChallengeEngine) -> Dictionary:
	var challenge := load_challenge(index, engine)
	if challenge == null:
		return {}
	return challenge.domain_config.get("showcase", {})
