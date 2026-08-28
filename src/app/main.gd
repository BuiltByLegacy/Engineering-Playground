extends Node2D

var playground: FlowPlayground
var solver: FlowLbmSolver
var visualizer: FlowVisualizer
var editor: FlowEditor
var challenge_engine := EngineeringChallengeEngine.new()
var telemetry := EngineeringRunTelemetry.new()
var current_challenge: EngineeringChallengeDefinition
var last_result: Dictionary = {}
var running := true
var elapsed := 0.0

const TOOLBAR_HEIGHT := 82.0
const BUTTON_W := 132.0
const BUTTON_H := 54.0

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
	_load_first_challenge()
	queue_redraw()

func _load_first_challenge() -> void:
	current_challenge = challenge_engine.load_json("res://content/flow/challenges/001_make_it_flow.json")
	if current_challenge == null:
		return
	var errors := challenge_engine.start_challenge(current_challenge)
	if not errors.is_empty():
		push_error("Challenge validation failed: %s" % errors)
		return
	telemetry.challenge_started(current_challenge)

func _process(delta: float) -> void:
	if running and solver != null and solver.is_ready():
		solver.step(delta)
		elapsed += delta
	queue_redraw()

func _unhandled_input(event: InputEvent) -> void:
	if event.is_action_pressed("run_simulation"):
		running = not running
		queue_redraw()
		return
	if event.is_action_pressed("reset_simulation"):
		editor.reset_scene()
		elapsed = 0.0
		last_result = {}
		queue_redraw()
		return

	if event is InputEventScreenTouch and event.pressed:
		if _handle_toolbar_tap(event.position):
			get_viewport().set_input_as_handled()
			return
	if event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
		if _handle_toolbar_tap(event.position):
			get_viewport().set_input_as_handled()
			return

	if editor != null and editor.handle_input(event):
		get_viewport().set_input_as_handled()
		queue_redraw()

func _score_run() -> void:
	if current_challenge == null or solver == null:
		return
	challenge_engine.begin_attempt()
	last_result = challenge_engine.evaluate(Callable(FlowChallengeEvaluator, "evaluate"), {"metrics": solver.get_metrics()})
	telemetry.attempt_scored(current_challenge, last_result, challenge_engine.get_time_to_first_run_seconds())
	queue_redraw()

func _handle_toolbar_tap(position: Vector2) -> bool:
	var viewport := get_viewport_rect().size
	if position.y <= TOOLBAR_HEIGHT:
		for i in range(tool_buttons.size()):
			var rect := Rect2(Vector2(18 + i * (BUTTON_W + 10), 14), Vector2(BUTTON_W, BUTTON_H))
			if rect.has_point(position):
				editor.set_tool(tool_buttons[i]["tool"])
				queue_redraw()
				return true
		var utility_x := viewport.x - (BUTTON_W + 10) * 3 - 18
		var undo_rect := Rect2(Vector2(utility_x, 14), Vector2(BUTTON_W, BUTTON_H))
		var redo_rect := Rect2(Vector2(utility_x + BUTTON_W + 10, 14), Vector2(BUTTON_W, BUTTON_H))
		var reset_rect := Rect2(Vector2(utility_x + (BUTTON_W + 10) * 2, 14), Vector2(BUTTON_W, BUTTON_H))
		if undo_rect.has_point(position): editor.undo(); return true
		if redo_rect.has_point(position): editor.redo(); return true
		if reset_rect.has_point(position):
			editor.reset_scene(); last_result = {}; elapsed = 0.0; return true

	if position.y >= viewport.y - TOOLBAR_HEIGHT:
		for i in range(view_buttons.size()):
			var rect := Rect2(Vector2(18 + i * (BUTTON_W + 10), viewport.y - 68), Vector2(BUTTON_W, BUTTON_H))
			if rect.has_point(position):
				visualizer.set_view_mode(view_buttons[i]["mode"])
				if current_challenge != null:
					telemetry.visualization_changed(current_challenge.challenge_id, view_buttons[i]["label"])
				return true
		var score_rect := Rect2(Vector2(viewport.x - (BUTTON_W + 10) * 2 - 18, viewport.y - 68), Vector2(BUTTON_W, BUTTON_H))
		var run_rect := Rect2(Vector2(viewport.x - BUTTON_W - 18, viewport.y - 68), Vector2(BUTTON_W, BUTTON_H))
		if score_rect.has_point(position):
			_score_run()
			return true
		if run_rect.has_point(position):
			running = not running
			return true
	return false

func _draw() -> void:
	var viewport := get_viewport_rect().size
	draw_rect(Rect2(Vector2.ZERO, Vector2(viewport.x, TOOLBAR_HEIGHT)), Color(0.035, 0.045, 0.065, 0.98), true)
	draw_rect(Rect2(Vector2(0, viewport.y - TOOLBAR_HEIGHT), Vector2(viewport.x, TOOLBAR_HEIGHT)), Color(0.035, 0.045, 0.065, 0.98), true)

	for i in range(tool_buttons.size()):
		var active := editor != null and editor.tool == tool_buttons[i]["tool"]
		_draw_button(Rect2(Vector2(18 + i * (BUTTON_W + 10), 14), Vector2(BUTTON_W, BUTTON_H)), tool_buttons[i]["label"], active)
	var utility_x := viewport.x - (BUTTON_W + 10) * 3 - 18
	_draw_button(Rect2(Vector2(utility_x, 14), Vector2(BUTTON_W, BUTTON_H)), "UNDO", false)
	_draw_button(Rect2(Vector2(utility_x + BUTTON_W + 10, 14), Vector2(BUTTON_W, BUTTON_H)), "REDO", false)
	_draw_button(Rect2(Vector2(utility_x + (BUTTON_W + 10) * 2, 14), Vector2(BUTTON_W, BUTTON_H)), "RESET", false)

	for i in range(view_buttons.size()):
		var active := visualizer != null and visualizer.view_mode == view_buttons[i]["mode"]
		_draw_button(Rect2(Vector2(18 + i * (BUTTON_W + 10), viewport.y - 68), Vector2(BUTTON_W, BUTTON_H)), view_buttons[i]["label"], active)
	_draw_button(Rect2(Vector2(viewport.x - (BUTTON_W + 10) * 2 - 18, viewport.y - 68), Vector2(BUTTON_W, BUTTON_H)), "SCORE", false)
	_draw_button(Rect2(Vector2(viewport.x - BUTTON_W - 18, viewport.y - 68), Vector2(BUTTON_W, BUTTON_H)), "PAUSE" if running else "RUN", running)

	var font := ThemeDB.fallback_font
	if current_challenge != null:
		draw_string(font, Vector2(450, 32), current_challenge.title, HORIZONTAL_ALIGNMENT_LEFT, -1, 22, Color(0.94, 0.97, 1.0))
		draw_string(font, Vector2(450, 58), current_challenge.description, HORIZONTAL_ALIGNMENT_LEFT, -1, 17, Color(0.70, 0.78, 0.88))

	if not last_result.is_empty():
		var panel := Rect2(Vector2(viewport.x - 450, 100), Vector2(420, 185))
		draw_rect(panel, Color(0.03, 0.045, 0.07, 0.93), true)
		var result_title := "%s  SCORE %.1f  GRADE %s" % ["PASSED" if last_result.get("success", false) else "KEEP TUNING", float(last_result.get("score", 0.0)), str(last_result.get("grade", ""))]
		draw_string(font, panel.position + Vector2(18, 32), result_title, HORIZONTAL_ALIGNMENT_LEFT, -1, 22, Color.WHITE)
		draw_string(font, panel.position + Vector2(18, 60), "Best %.1f   Change %+.1f" % [float(last_result.get("best_score", 0.0)), float(last_result.get("improvement", 0.0))], HORIZONTAL_ALIGNMENT_LEFT, -1, 17, Color(0.72, 0.84, 0.96))
		var feedback: PackedStringArray = last_result.get("feedback", PackedStringArray())
		for i in range(min(3, feedback.size())):
			draw_string(font, panel.position + Vector2(18, 91 + i * 27), feedback[i], HORIZONTAL_ALIGNMENT_LEFT, -1, 16, Color(0.86, 0.90, 0.94))

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
