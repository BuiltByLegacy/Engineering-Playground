extends Node2D

var playground: FlowPlayground
var solver: EngineeringSimulationAdapter
var running := true
var elapsed := 0.0

func _ready() -> void:
	playground = FlowPlayground.new()
	solver = playground.create_simulation_adapter()
	queue_redraw()

func _process(delta: float) -> void:
	if running and solver != null and solver.is_ready():
		solver.step(delta)
		elapsed += delta
		queue_redraw()

func _unhandled_input(event: InputEvent) -> void:
	if event.is_action_pressed("run_simulation"):
		running = not running
	elif event.is_action_pressed("reset_simulation"):
		solver.reset()
		elapsed = 0.0
		queue_redraw()

func _draw() -> void:
	if solver == null or not solver.is_ready():
		return
	var data := solver.get_visualization_data()
	var width: int = data["width"]
	var height: int = data["height"]
	var vx: PackedFloat64Array = data["velocity_x"]
	var vy: PackedFloat64Array = data["velocity_y"]
	var solid: PackedByteArray = data["solid"]

	var viewport_size := get_viewport_rect().size
	var cell_size := min(viewport_size.x / float(width), viewport_size.y / float(height))
	var origin := (viewport_size - Vector2(width, height) * cell_size) * 0.5

	for y in range(height):
		for x in range(width):
			var c := y * width + x
			var rect := Rect2(origin + Vector2(x, y) * cell_size, Vector2.ONE * (cell_size + 0.5))
			if solid[c] == 1:
				draw_rect(rect, Color(0.08, 0.10, 0.14), true)
			else:
				var speed := sqrt(vx[c] * vx[c] + vy[c] * vy[c])
				var intensity := clamp(speed / 0.12, 0.0, 1.0)
				var color := Color(0.08 + 0.12 * intensity, 0.24 + 0.45 * intensity, 0.72 + 0.25 * intensity)
				draw_rect(rect, color, true)

	var metrics := solver.get_metrics()
	var font := ThemeDB.fallback_font
	var label := "%s  |  SPACE pause/run  |  R reset  |  outlet %.4f" % [playground.get_display_name(), metrics["mean_outlet_speed"]]
	draw_string(font, Vector2(28, 42), label, HORIZONTAL_ALIGNMENT_LEFT, -1, 24, Color.WHITE)
