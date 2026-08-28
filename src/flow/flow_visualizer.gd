class_name FlowVisualizer
extends Node2D

enum ViewMode { PARTICLES, VELOCITY, PRESSURE, TURBULENCE }

var solver: EngineeringSimulationAdapter
var view_mode: ViewMode = ViewMode.PARTICLES
var camera_offset := Vector2.ZERO
var zoom := 1.0
var particle_count := 220
var particles: Array[Vector2] = []
var _rng := RandomNumberGenerator.new()

func set_solver(value: EngineeringSimulationAdapter) -> void:
	solver = value
	_rng.seed = 424242
	_reset_particles()
	queue_redraw()

func set_view_mode(mode: ViewMode) -> void:
	view_mode = mode
	queue_redraw()

func set_camera(offset: Vector2, scale: float) -> void:
	camera_offset = offset
	zoom = clamp(scale, 0.65, 2.5)
	queue_redraw()

func _process(delta: float) -> void:
	if solver == null or not solver.is_ready():
		return
	if view_mode == ViewMode.PARTICLES:
		_update_particles(delta)
	queue_redraw()

func get_layout() -> Dictionary:
	if solver == null or not solver.is_ready():
		return {}
	var data := solver.get_visualization_data()
	var width: int = data["width"]
	var height: int = data["height"]
	var viewport_size := get_viewport_rect().size
	var base_cell := min(viewport_size.x / float(width), (viewport_size.y - 150.0) / float(height))
	var cell_size := base_cell * zoom
	var field_size := Vector2(width, height) * cell_size
	var origin := (viewport_size - field_size) * 0.5 + camera_offset + Vector2(0, 18)
	return {"origin": origin, "cell_size": cell_size, "width": width, "height": height}

func screen_to_grid(position: Vector2) -> Vector2i:
	var layout := get_layout()
	if layout.is_empty():
		return Vector2i(-1, -1)
	var local := (position - layout["origin"]) / float(layout["cell_size"])
	return Vector2i(floori(local.x), floori(local.y))

func grid_to_screen(cell: Vector2) -> Vector2:
	var layout := get_layout()
	if layout.is_empty():
		return Vector2.ZERO
	return layout["origin"] + (cell + Vector2(0.5, 0.5)) * float(layout["cell_size"])

func _draw() -> void:
	if solver == null or not solver.is_ready():
		return
	var data := solver.get_visualization_data()
	var layout := get_layout()
	var width: int = data["width"]
	var height: int = data["height"]
	var rho: PackedFloat64Array = data["density"]
	var vx: PackedFloat64Array = data["velocity_x"]
	var vy: PackedFloat64Array = data["velocity_y"]
	var solid: PackedByteArray = data["solid"]
	var origin: Vector2 = layout["origin"]
	var cell_size: float = layout["cell_size"]

	for y in range(height):
		for x in range(width):
			var c := y * width + x
			var rect := Rect2(origin + Vector2(x, y) * cell_size, Vector2.ONE * (cell_size + 0.5))
			if solid[c] == 1:
				draw_rect(rect, Color(0.07, 0.08, 0.11), true)
				continue
			match view_mode:
				ViewMode.VELOCITY:
					var speed := sqrt(vx[c] * vx[c] + vy[c] * vy[c])
					var t := clamp(speed / 0.12, 0.0, 1.0)
					draw_rect(rect, Color(0.05 + 0.15 * t, 0.16 + 0.55 * t, 0.35 + 0.6 * t), true)
				ViewMode.PRESSURE:
					var p := clamp((rho[c] - 0.94) / 0.12, 0.0, 1.0)
					draw_rect(rect, Color(0.1 + 0.75 * p, 0.16 + 0.20 * (1.0 - abs(p - 0.5) * 2.0), 0.78 - 0.62 * p), true)
				ViewMode.TURBULENCE:
					var vort := _vorticity(x, y, width, height, vx, vy)
					var t := clamp(abs(vort) / 0.035, 0.0, 1.0)
					draw_rect(rect, Color(0.07 + 0.7 * t, 0.10 + 0.16 * (1.0 - t), 0.18 + 0.25 * t), true)
				_:
					draw_rect(rect, Color(0.045, 0.12, 0.22), true)

	if view_mode == ViewMode.PARTICLES:
		for p in particles:
			draw_circle(grid_to_screen(p), max(1.6, cell_size * 0.16), Color(0.70, 0.94, 1.0, 0.92))

	_draw_legend(view_mode)

func _draw_legend(mode: ViewMode) -> void:
	var font := ThemeDB.fallback_font
	var label := "TRACERS"
	match mode:
		ViewMode.VELOCITY: label = "VELOCITY  slow → fast"
		ViewMode.PRESSURE: label = "PRESSURE  low → high"
		ViewMode.TURBULENCE: label = "SWIRL / RECIRCULATION  low → high"
	draw_string(font, Vector2(28, get_viewport_rect().size.y - 24), label, HORIZONTAL_ALIGNMENT_LEFT, -1, 20, Color(0.88, 0.93, 1.0))

func _update_particles(delta: float) -> void:
	var data := solver.get_visualization_data()
	var width: int = data["width"]
	var height: int = data["height"]
	var vx: PackedFloat64Array = data["velocity_x"]
	var vy: PackedFloat64Array = data["velocity_y"]
	var solid: PackedByteArray = data["solid"]
	var speed_scale := 42.0 * delta
	for i in range(particles.size()):
		var p := particles[i]
		var x := clampi(int(p.x), 0, width - 1)
		var y := clampi(int(p.y), 0, height - 1)
		var c := y * width + x
		if solid[c] == 1 or p.x >= width - 2 or p.x < 1 or p.y < 1 or p.y >= height - 1:
			particles[i] = _spawn_particle(width, height, solid)
			continue
		p += Vector2(vx[c], vy[c]) * speed_scale
		particles[i] = p

func _reset_particles() -> void:
	particles.clear()
	if solver == null or not solver.is_ready():
		return
	var data := solver.get_visualization_data()
	for _i in range(particle_count):
		particles.append(_spawn_particle(data["width"], data["height"], data["solid"]))

func _spawn_particle(width: int, height: int, solid: PackedByteArray) -> Vector2:
	for _attempt in range(20):
		var y := _rng.randf_range(1.5, height - 2.5)
		var x := _rng.randf_range(1.2, 4.5)
		if solid[int(y) * width + int(x)] == 0:
			return Vector2(x, y)
	return Vector2(2.0, height * 0.5)

func _vorticity(x: int, y: int, width: int, height: int, vx: PackedFloat64Array, vy: PackedFloat64Array) -> float:
	if x <= 0 or x >= width - 1 or y <= 0 or y >= height - 1:
		return 0.0
	var dvy_dx := (vy[y * width + x + 1] - vy[y * width + x - 1]) * 0.5
	var dvx_dy := (vx[(y + 1) * width + x] - vx[(y - 1) * width + x]) * 0.5
	return dvy_dx - dvx_dy
