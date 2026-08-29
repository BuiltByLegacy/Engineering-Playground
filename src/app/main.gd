extends Node2D

enum AppMode { CHALLENGE, SHOWCASE, SANDBOX, LEARN }

var playground: FlowPlayground
var solver: FlowLbmSolver
var visualizer: FlowVisualizer
var editor: FlowEditor
var challenge_engine := EngineeringChallengeEngine.new()
var telemetry := EngineeringRunTelemetry.new()
var progression := EngineeringProgressionStore.new()
var learn_catalog := EngineeringLearnCatalog.new()
var campaign := EngineeringCampaignCatalog.new()
var showcase_catalog := FlowShowcaseCatalog.new()
var current_challenge: EngineeringChallengeDefinition
var current_campaign_index := 0
var current_showcase_index := 0
var last_result: Dictionary = {}
var app_mode: AppMode = AppMode.CHALLENGE
var running := true
var elapsed := 0.0

const TOOLBAR_HEIGHT := 82.0
const BUTTON_W := 132.0
const BUTTON_H := 54.0
const MODE_BUTTON_W := 118.0
const NAV_BUTTON_H := 42.0

var mode_buttons := [
	{"label":"CHALLENGE","mode":AppMode.CHALLENGE},
	{"label":"SHOWCASE","mode":AppMode.SHOWCASE},
	{"label":"SANDBOX","mode":AppMode.SANDBOX},
	{"label":"LEARN","mode":AppMode.LEARN},
]

var tool_buttons := [
	{"label": "DRAW", "tool": FlowEditor.Tool.DRAW_WALL},
	{"label": "ERASE", "tool": FlowEditor.Tool.ERASE},
	{"label": "PAN", "tool": FlowEditor.Tool.PAN},
]

var view_buttons := [
	{"label": "FLOW", "mode": FlowVisualizer.ViewMode.PARTICLES},
	{"label": "SPEED", "mode": FlowVisualizer.ViewMode.VELOCITY},
	{"label": "PRESS", "mode": FlowVisualizer.ViewMode.PRESSURE},
	{"label": "SWIRL", "mode": FlowVisualizer.ViewMode.TURBULENCE},
]

func _ready() -> void:
	playground = FlowPlayground.new()
	solver = playground.create_simulation_adapter() as FlowLbmSolver
	visualizer = FlowVisualizer.new()
	add_child(visualizer)
	visualizer.set_solver(solver)
	editor = FlowEditor.new()
	editor.setup(solver, visualizer)
	_load_showcases()
	_load_campaign()
	queue_redraw()

func _load_showcases() -> void:
	var errors := showcase_catalog.load_json()
	if not errors.is_empty():
		push_error("Showcase validation failed: %s" % errors)

func _load_campaign() -> void:
	var errors := campaign.load_json("res://content/flow/campaign.json")
	if not errors.is_empty():
		push_error("Campaign validation failed: %s" % errors)
		return
	_load_challenge_index(0)

func _start_challenge(challenge: EngineeringChallengeDefinition) -> bool:
	if challenge == null:
		return false
	current_challenge = challenge
	var errors := challenge_engine.start_challenge(current_challenge)
	if not errors.is_empty():
		push_error("Challenge validation failed: %s" % errors)
		return false
	last_result = {}
	elapsed = 0.0
	editor.reset_scene()
	telemetry.challenge_started(current_challenge)
	queue_redraw()
	return true

func _load_challenge_index(index: int) -> void:
	if campaign.size() == 0:
		return
	index = clampi(index, 0, campaign.size() - 1)
	if not _challenge_is_unlocked(index):
		return
	var challenge := campaign.get_challenge(index)
	if _start_challenge(challenge):
		current_campaign_index = index

func _load_showcase_index(index: int) -> void:
	if showcase_catalog.size() == 0:
		return
	index = clampi(index, 0, showcase_catalog.size() - 1)
	var challenge := showcase_catalog.load_challenge(index, challenge_engine)
	if _start_challenge(challenge):
		current_showcase_index = index

func _challenge_is_unlocked(index: int) -> bool:
	if index <= 0:
		return true
	if not campaign.is_unlocked(index, progression.total_stars()):
		return false
	var previous := campaign.get_challenge(index - 1)
	if previous == null:
		return false
	return bool(progression.get_challenge_progress(previous.challenge_id).get("completed", false))

func _change_challenge(direction: int) -> void:
	if app_mode != AppMode.CHALLENGE or campaign.size() == 0:
		return
	var candidate := clampi(current_campaign_index + direction, 0, campaign.size() - 1)
	if candidate == current_campaign_index:
		return
	if direction > 0 and not _challenge_is_unlocked(candidate):
		return
	_load_challenge_index(candidate)

func _change_showcase(direction: int) -> void:
	if app_mode != AppMode.SHOWCASE or showcase_catalog.size() == 0:
		return
	var candidate := clampi(current_showcase_index + direction, 0, showcase_catalog.size() - 1)
	if candidate != current_showcase_index:
		_load_showcase_index(candidate)

func _process(delta: float) -> void:
	if app_mode != AppMode.LEARN and running and solver != null and solver.is_ready():
		solver.step(delta)
		elapsed += delta
	queue_redraw()

func _unhandled_input(event: InputEvent) -> void:
	if event.is_action_pressed("run_simulation") and app_mode != AppMode.LEARN:
		running = not running
		queue_redraw()
		return
	if event.is_action_pressed("reset_simulation"):
		_reset_current_mode()
		return
	if event is InputEventScreenTouch and event.pressed:
		if _handle_toolbar_tap(event.position):
			get_viewport().set_input_as_handled()
			return
	if event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
		if _handle_toolbar_tap(event.position):
			get_viewport().set_input_as_handled()
			return
	if app_mode != AppMode.LEARN and editor != null and editor.handle_input(event):
		get_viewport().set_input_as_handled()
		queue_redraw()

func _score_run() -> void:
	if app_mode not in [AppMode.CHALLENGE, AppMode.SHOWCASE] or current_challenge == null or solver == null:
		return
	challenge_engine.begin_attempt()
	last_result = challenge_engine.evaluate(Callable(self, "_evaluate_challenge"), {"metrics": solver.get_metrics()})
	telemetry.attempt_scored(current_challenge, last_result, challenge_engine.get_time_to_first_run_seconds())
	if app_mode == AppMode.CHALLENGE and bool(last_result.get("success", false)):
		progression.record_result(current_challenge, last_result)
	queue_redraw()

func _evaluate_challenge(challenge: EngineeringChallengeDefinition, context: Dictionary) -> Dictionary:
	return FlowChallengeEvaluator.evaluate(challenge, context)

func _switch_mode(mode: AppMode) -> void:
	if app_mode == mode:
		return
	app_mode = mode
	last_result = {}
	elapsed = 0.0
	match app_mode:
		AppMode.SANDBOX:
			editor.reset_blank_scene(); running = true
		AppMode.CHALLENGE:
			_load_challenge_index(current_campaign_index); running = true
		AppMode.SHOWCASE:
			_load_showcase_index(current_showcase_index); running = true
		AppMode.LEARN:
			running = false
	queue_redraw()

func _reset_current_mode() -> void:
	if app_mode == AppMode.SANDBOX:
		editor.reset_blank_scene()
	elif app_mode in [AppMode.CHALLENGE, AppMode.SHOWCASE]:
		editor.reset_scene()
	last_result = {}
	elapsed = 0.0
	queue_redraw()

func _toggle_presentation_mode() -> void:
	var next := "engineer" if progression.get_presentation_mode() == "explorer" else "explorer"
	progression.set_presentation_mode(next)
	queue_redraw()

func _handle_toolbar_tap(position: Vector2) -> bool:
	var viewport := get_viewport_rect().size
	if position.y <= TOOLBAR_HEIGHT:
		for i in range(mode_buttons.size()):
			var mode_rect := Rect2(Vector2(18 + i * (MODE_BUTTON_W + 8), 14), Vector2(MODE_BUTTON_W, BUTTON_H))
			if mode_rect.has_point(position):
				_switch_mode(mode_buttons[i]["mode"])
				return true
		if app_mode != AppMode.LEARN:
			var tool_start := 540.0
			for i in range(tool_buttons.size()):
				var rect := Rect2(Vector2(tool_start + i * (BUTTON_W + 10), 14), Vector2(BUTTON_W, BUTTON_H))
				if rect.has_point(position):
					editor.set_tool(tool_buttons[i]["tool"])
					return true
		var utility_x := viewport.x - (BUTTON_W + 10) * 3 - 18
		var undo_rect := Rect2(Vector2(utility_x, 14), Vector2(BUTTON_W, BUTTON_H))
		var redo_rect := Rect2(Vector2(utility_x + BUTTON_W + 10, 14), Vector2(BUTTON_W, BUTTON_H))
		var reset_rect := Rect2(Vector2(utility_x + (BUTTON_W + 10) * 2, 14), Vector2(BUTTON_W, BUTTON_H))
		if app_mode != AppMode.LEARN and undo_rect.has_point(position): editor.undo(); return true
		if app_mode != AppMode.LEARN and redo_rect.has_point(position): editor.redo(); return true
		if reset_rect.has_point(position): _reset_current_mode(); return true

	if app_mode == AppMode.CHALLENGE:
		if Rect2(Vector2(28, 184), Vector2(BUTTON_W, NAV_BUTTON_H)).has_point(position): _change_challenge(-1); return true
		if Rect2(Vector2(170, 184), Vector2(BUTTON_W, NAV_BUTTON_H)).has_point(position): _change_challenge(1); return true
	elif app_mode == AppMode.SHOWCASE:
		if Rect2(Vector2(28, 184), Vector2(BUTTON_W, NAV_BUTTON_H)).has_point(position): _change_showcase(-1); return true
		if Rect2(Vector2(170, 184), Vector2(BUTTON_W, NAV_BUTTON_H)).has_point(position): _change_showcase(1); return true

	if app_mode == AppMode.LEARN:
		var mode_rect := Rect2(Vector2(viewport.x - BUTTON_W - 18, viewport.y - 68), Vector2(BUTTON_W, BUTTON_H))
		if mode_rect.has_point(position): _toggle_presentation_mode(); return true
		return false

	if position.y >= viewport.y - TOOLBAR_HEIGHT:
		for i in range(view_buttons.size()):
			var rect := Rect2(Vector2(18 + i * (BUTTON_W + 10), viewport.y - 68), Vector2(BUTTON_W, BUTTON_H))
			if rect.has_point(position):
				visualizer.set_view_mode(view_buttons[i]["mode"])
				if current_challenge != null and app_mode in [AppMode.CHALLENGE, AppMode.SHOWCASE]:
					telemetry.visualization_changed(current_challenge.challenge_id, view_buttons[i]["label"])
				return true
		var score_rect := Rect2(Vector2(viewport.x - (BUTTON_W + 10) * 2 - 18, viewport.y - 68), Vector2(BUTTON_W, BUTTON_H))
		var run_rect := Rect2(Vector2(viewport.x - BUTTON_W - 18, viewport.y - 68), Vector2(BUTTON_W, BUTTON_H))
		if app_mode in [AppMode.CHALLENGE, AppMode.SHOWCASE] and score_rect.has_point(position): _score_run(); return true
		if app_mode == AppMode.SANDBOX and score_rect.has_point(position): _reset_current_mode(); return true
		if run_rect.has_point(position): running = not running; return true
	return false

func _draw() -> void:
	var viewport := get_viewport_rect().size
	if app_mode == AppMode.SHOWCASE:
		_draw_showcase_environment(viewport)
	draw_rect(Rect2(Vector2.ZERO, Vector2(viewport.x, TOOLBAR_HEIGHT)), Color(0.035, 0.045, 0.065, 0.98), true)
	draw_rect(Rect2(Vector2(0, viewport.y - TOOLBAR_HEIGHT), Vector2(viewport.x, TOOLBAR_HEIGHT)), Color(0.035, 0.045, 0.065, 0.98), true)
	for i in range(mode_buttons.size()):
		_draw_button(Rect2(Vector2(18 + i * (MODE_BUTTON_W + 8), 14), Vector2(MODE_BUTTON_W, BUTTON_H)), mode_buttons[i]["label"], app_mode == mode_buttons[i]["mode"])

	if app_mode != AppMode.LEARN:
		var tool_start := 540.0
		for i in range(tool_buttons.size()):
			var active := editor != null and editor.tool == tool_buttons[i]["tool"]
			_draw_button(Rect2(Vector2(tool_start + i * (BUTTON_W + 10), 14), Vector2(BUTTON_W, BUTTON_H)), tool_buttons[i]["label"], active)
		var utility_x := viewport.x - (BUTTON_W + 10) * 3 - 18
		_draw_button(Rect2(Vector2(utility_x, 14), Vector2(BUTTON_W, BUTTON_H)), "UNDO", false)
		_draw_button(Rect2(Vector2(utility_x + BUTTON_W + 10, 14), Vector2(BUTTON_W, BUTTON_H)), "REDO", false)
		_draw_button(Rect2(Vector2(utility_x + (BUTTON_W + 10) * 2, 14), Vector2(BUTTON_W, BUTTON_H)), "RESET", false)

	var font := ThemeDB.fallback_font
	if app_mode == AppMode.LEARN:
		_draw_learn_mode(viewport, font)
		return

	for i in range(view_buttons.size()):
		var active := visualizer != null and visualizer.view_mode == view_buttons[i]["mode"]
		_draw_button(Rect2(Vector2(18 + i * (BUTTON_W + 10), viewport.y - 68), Vector2(BUTTON_W, BUTTON_H)), view_buttons[i]["label"], active)
	_draw_button(Rect2(Vector2(viewport.x - (BUTTON_W + 10) * 2 - 18, viewport.y - 68), Vector2(BUTTON_W, BUTTON_H)), "SCORE" if app_mode in [AppMode.CHALLENGE, AppMode.SHOWCASE] else "CLEAR", false)
	_draw_button(Rect2(Vector2(viewport.x - BUTTON_W - 18, viewport.y - 68), Vector2(BUTTON_W, BUTTON_H)), "PAUSE" if running else "RUN", running)

	if app_mode == AppMode.CHALLENGE and current_challenge != null:
		_draw_campaign_header(font)
	elif app_mode == AppMode.SHOWCASE and current_challenge != null:
		_draw_showcase_header(viewport, font)
	elif app_mode == AppMode.SANDBOX:
		draw_string(font, Vector2(28, 112), "Flow Lab Sandbox", HORIZONTAL_ALIGNMENT_LEFT, -1, 24, Color(0.94, 0.97, 1.0))
		draw_string(font, Vector2(28, 140), "No score. No objective. Draw, erase, run, and experiment.", HORIZONTAL_ALIGNMENT_LEFT, -1, 18, Color(0.70, 0.78, 0.88))

	if not last_result.is_empty():
		_draw_result_panel(viewport, font)

func _draw_campaign_header(font: Font) -> void:
	var meta := campaign.get_meta(current_campaign_index)
	draw_string(font, Vector2(28, 108), "Chapter %d — %s   Level %d/%d" % [int(meta.get("chapter_number", 1)), str(meta.get("chapter_title", "")), current_campaign_index + 1, campaign.size()], HORIZONTAL_ALIGNMENT_LEFT, -1, 17, Color(0.62, 0.73, 0.86))
	draw_string(font, Vector2(28, 137), current_challenge.title, HORIZONTAL_ALIGNMENT_LEFT, -1, 24, Color(0.94, 0.97, 1.0))
	draw_string(font, Vector2(28, 163), current_challenge.description, HORIZONTAL_ALIGNMENT_LEFT, -1, 17, Color(0.70, 0.78, 0.88))
	_draw_button(Rect2(Vector2(28, 184), Vector2(BUTTON_W, NAV_BUTTON_H)), "PREV", current_campaign_index > 0)
	var next_unlocked := current_campaign_index < campaign.size() - 1 and _challenge_is_unlocked(current_campaign_index + 1)
	_draw_button(Rect2(Vector2(170, 184), Vector2(BUTTON_W, NAV_BUTTON_H)), "NEXT", next_unlocked)
	var p := progression.get_challenge_progress(current_challenge.challenge_id)
	var targets: Array = current_challenge.rewards.get("target_scores", [])
	var target_text := ""
	if targets.size() >= 3: target_text = "   Targets %d / %d / %d" % [int(targets[0]), int(targets[1]), int(targets[2])]
	draw_string(font, Vector2(320, 213), "Stars %d/3   Best %.1f   Total %d%s" % [int(p["stars"]), float(p["best_score"]), progression.total_stars(), target_text], HORIZONTAL_ALIGNMENT_LEFT, -1, 16, Color(0.72, 0.84, 0.96))

func _draw_showcase_header(viewport: Vector2, font: Font) -> void:
	var entry := showcase_catalog.get_entry(current_showcase_index)
	var theme := str(entry.get("theme", ""))
	draw_string(font, Vector2(28, 108), "APPLIED FLOW SHOWCASE   %d/%d" % [current_showcase_index + 1, showcase_catalog.size()], HORIZONTAL_ALIGNMENT_LEFT, -1, 17, Color(0.62, 0.73, 0.86))
	draw_string(font, Vector2(28, 137), current_challenge.title, HORIZONTAL_ALIGNMENT_LEFT, -1, 28, Color.WHITE)
	draw_string(font, Vector2(28, 166), current_challenge.description, HORIZONTAL_ALIGNMENT_LEFT, -1, 17, Color(0.78, 0.84, 0.90))
	_draw_button(Rect2(Vector2(28, 184), Vector2(BUTTON_W, NAV_BUTTON_H)), "PREV", current_showcase_index > 0)
	_draw_button(Rect2(Vector2(170, 184), Vector2(BUTTON_W, NAV_BUTTON_H)), "NEXT", current_showcase_index < showcase_catalog.size() - 1)
	_draw_reference_math_panel(viewport, font, theme)

func _draw_reference_math_panel(viewport: Vector2, font: Font, theme: String) -> void:
	var reference := FlowEngineeringReferenceModel.default_reference_for_theme(theme)
	var math := FlowEngineeringReferenceModel.total_reference_pressure_loss(reference)
	var panel := Rect2(Vector2(viewport.x - 430, 305), Vector2(400, 215))
	draw_rect(panel, Color(0.025, 0.04, 0.06, 0.94), true)
	draw_string(font, panel.position + Vector2(18, 30), "REFERENCE MATH", HORIZONTAL_ALIGNMENT_LEFT, -1, 20, Color.WHITE)
	draw_string(font, panel.position + Vector2(18, 58), "%s • Dₕ %.0f mm • V %.1f m/s" % [str(reference.get("fluid", "fluid")).capitalize(), float(reference.get("hydraulic_diameter_m", 0.0)) * 1000.0, float(reference.get("reference_velocity_mps", 0.0))], HORIZONTAL_ALIGNMENT_LEFT, -1, 15, Color(0.73, 0.82, 0.90))
	draw_string(font, panel.position + Vector2(18, 87), "Re %.0f   %s   f %.4f" % [float(math.get("reynolds", 0.0)), str(math.get("flow_regime", "")), float(math.get("friction_factor", 0.0))], HORIZONTAL_ALIGNMENT_LEFT, -1, 16, Color(0.90, 0.93, 0.96))
	draw_string(font, panel.position + Vector2(18, 116), "Q %.3f m³/s   q %.1f Pa" % [float(math.get("flow_m3_s", 0.0)), float(math.get("dynamic_pressure_pa", 0.0))], HORIZONTAL_ALIGNMENT_LEFT, -1, 16, Color(0.90, 0.93, 0.96))
	draw_string(font, panel.position + Vector2(18, 145), "ΔP major %.1f Pa   minor %.1f Pa" % [float(math.get("major_loss_pa", 0.0)), float(math.get("minor_loss_pa", 0.0))], HORIZONTAL_ALIGNMENT_LEFT, -1, 16, Color(0.90, 0.93, 0.96))
	draw_string(font, panel.position + Vector2(18, 174), "Reference estimate — not calibrated CFD", HORIZONTAL_ALIGNMENT_LEFT, -1, 14, Color(0.92, 0.68, 0.45))
	draw_string(font, panel.position + Vector2(18, 198), "Q=AV   Re=ρVD/μ   ΔP=f(L/D)ρV²/2 + KρV²/2", HORIZONTAL_ALIGNMENT_LEFT, -1, 13, Color(0.68, 0.76, 0.84))

func _draw_showcase_environment(viewport: Vector2) -> void:
	var entry := showcase_catalog.get_entry(current_showcase_index)
	var theme := str(entry.get("theme", ""))
	var area := Rect2(Vector2(330, 280), Vector2(viewport.x - 780, viewport.y - 470))
	match theme:
		"residential_plumbing":
			draw_rect(area, Color(0.08, 0.15, 0.20, 0.20), true)
			draw_polyline(PackedVector2Array([area.position + Vector2(40, 100), area.position + Vector2(180, 20), area.position + Vector2(320, 100), area.position + Vector2(320, 300), area.position + Vector2(40, 300), area.position + Vector2(40, 100)]), Color(0.65,0.75,0.82,0.35), 5.0)
			draw_string(ThemeDB.fallback_font, area.position + Vector2(70, 265), "SUPPLY → SHOWER", HORIZONTAL_ALIGNMENT_LEFT, -1, 18, Color(0.55,0.82,1.0,0.55))
		"automotive_exhaust":
			draw_rect(area, Color(0.18, 0.09, 0.06, 0.18), true)
			draw_polyline(PackedVector2Array([area.position + Vector2(40,220), area.position + Vector2(120,130), area.position + Vector2(380,130), area.position + Vector2(470,220), area.position + Vector2(40,220)]), Color(0.82,0.60,0.48,0.38), 7.0)
			draw_string(ThemeDB.fallback_font, area.position + Vector2(65, 265), "ENGINE → PACKAGING → REAR EXIT", HORIZONTAL_ALIGNMENT_LEFT, -1, 18, Color(1.0,0.70,0.48,0.55))
		"hvac_distribution":
			draw_rect(area, Color(0.07, 0.12, 0.17, 0.20), true)
			for i in range(3): draw_rect(Rect2(area.position + Vector2(40 + i * 155, 80), Vector2(135, 180)), Color(0.4,0.65,0.8,0.10), false, 3.0)
			draw_string(ThemeDB.fallback_font, area.position + Vector2(55, 300), "AIR HANDLER → ROOM A / B / C", HORIZONTAL_ALIGNMENT_LEFT, -1, 18, Color(0.60,0.82,1.0,0.55))
		"manifold_optimization":
			draw_rect(area, Color(0.10, 0.08, 0.17, 0.18), true)
			draw_rect(Rect2(area.position + Vector2(180, 110), Vector2(150, 100)), Color(0.55,0.42,0.8,0.15), false, 5.0)
			for y in [80.0, 130.0, 180.0, 230.0]: draw_line(area.position + Vector2(330,160), area.position + Vector2(470,y), Color(0.70,0.58,1.0,0.4), 4.0)
			draw_string(ThemeDB.fallback_font, area.position + Vector2(90, 290), "ONE INLET → FOUR BALANCED OUTLETS", HORIZONTAL_ALIGNMENT_LEFT, -1, 18, Color(0.76,0.65,1.0,0.55))

func _draw_result_panel(viewport: Vector2, font: Font) -> void:
	var panel := Rect2(Vector2(viewport.x - 450, 100), Vector2(420, 185))
	draw_rect(panel, Color(0.03, 0.045, 0.07, 0.93), true)
	var result_title := "%s  SCORE %.1f  GRADE %s" % ["PASSED" if last_result.get("success", false) else "KEEP TUNING", float(last_result.get("score", 0.0)), str(last_result.get("grade", ""))]
	draw_string(font, panel.position + Vector2(18, 32), result_title, HORIZONTAL_ALIGNMENT_LEFT, -1, 22, Color.WHITE)
	draw_string(font, panel.position + Vector2(18, 60), "Best %.1f   Change %+.1f" % [float(last_result.get("best_score", 0.0)), float(last_result.get("improvement", 0.0))], HORIZONTAL_ALIGNMENT_LEFT, -1, 17, Color(0.72, 0.84, 0.96))
	var feedback: PackedStringArray = last_result.get("feedback", PackedStringArray())
	for i in range(min(3, feedback.size())):
		draw_string(font, panel.position + Vector2(18, 91 + i * 27), feedback[i], HORIZONTAL_ALIGNMENT_LEFT, -1, 16, Color(0.86, 0.90, 0.94))

func _draw_learn_mode(viewport: Vector2, font: Font) -> void:
	var mode := progression.get_presentation_mode()
	var unlocked := progression.get_unlocked_concepts()
	var cards := learn_catalog.get_unlocked_cards(unlocked, mode)
	draw_string(font, Vector2(38, 128), "Learn Library", HORIZONTAL_ALIGNMENT_LEFT, -1, 32, Color.WHITE)
	draw_string(font, Vector2(38, 162), "%d concepts discovered — %s mode" % [cards.size(), mode.capitalize()], HORIZONTAL_ALIGNMENT_LEFT, -1, 18, Color(0.72, 0.84, 0.96))
	if cards.is_empty():
		draw_string(font, Vector2(38, 215), "Complete challenges to discover engineering concepts here.", HORIZONTAL_ALIGNMENT_LEFT, -1, 20, Color(0.82, 0.86, 0.90))
	else:
		for i in range(min(cards.size(), 6)):
			var card: Dictionary = cards[i]
			var y := 205.0 + i * 112.0
			var rect := Rect2(Vector2(38, y), Vector2(min(1100.0, viewport.x - 76.0), 92))
			draw_rect(rect, Color(0.055, 0.075, 0.105, 0.96), true)
			draw_string(font, rect.position + Vector2(18, 30), str(card["title"]), HORIZONTAL_ALIGNMENT_LEFT, -1, 21, Color.WHITE)
			draw_string(font, rect.position + Vector2(18, 61), str(card["body"]), HORIZONTAL_ALIGNMENT_LEFT, -1, 17, Color(0.78, 0.84, 0.90))
	_draw_button(Rect2(Vector2(viewport.x - BUTTON_W - 18, viewport.y - 68), Vector2(BUTTON_W, BUTTON_H)), mode.to_upper(), true)

func _draw_button(rect: Rect2, label: String, active: bool) -> void:
	var bg := Color(0.12, 0.30, 0.50) if active else Color(0.10, 0.13, 0.18)
	draw_style_box(_button_style(bg), rect)
	var font := ThemeDB.fallback_font
	var text_size := font.get_string_size(label, HORIZONTAL_ALIGNMENT_CENTER, -1, 19)
	var pos := rect.position + Vector2((rect.size.x - text_size.x) * 0.5, (rect.size.y + text_size.y) * 0.5 - 4)
	draw_string(font, pos, label, HORIZONTAL_ALIGNMENT_LEFT, -1, 19, Color.WHITE)

func _button_style(color: Color) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = color
	style.corner_radius_top_left = 10
	style.corner_radius_top_right = 10
	style.corner_radius_bottom_left = 10
	style.corner_radius_bottom_right = 10
	return style
