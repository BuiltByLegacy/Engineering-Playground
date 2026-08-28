class_name FlowEditor
extends RefCounted

enum Tool { DRAW_WALL, ERASE, PAN }

var solver: FlowLbmSolver
var visualizer: FlowVisualizer
var tool: Tool = Tool.DRAW_WALL
var brush_radius := 1
var camera_offset := Vector2.ZERO
var camera_zoom := 1.0

var _drawing := false
var _panning := false
var _last_screen := Vector2.ZERO
var _last_grid := Vector2i(-1, -1)
var _active_touches: Dictionary = {}
var _pinch_start_distance := 0.0
var _pinch_start_zoom := 1.0
var _undo_stack: Array[PackedByteArray] = []
var _redo_stack: Array[PackedByteArray] = []

func setup(value_solver: FlowLbmSolver, value_visualizer: FlowVisualizer) -> void:
	solver = value_solver
	visualizer = value_visualizer
	camera_offset = Vector2.ZERO
	camera_zoom = 1.0
	visualizer.set_camera(camera_offset, camera_zoom)

func set_tool(value: Tool) -> void:
	tool = value
	_drawing = false
	_panning = false

func undo() -> void:
	if _undo_stack.is_empty() or solver == null:
		return
	_redo_stack.append(solver.get_solid_mask())
	solver.apply_solid_mask(_undo_stack.pop_back())
	solver.reset()
	visualizer.queue_redraw()

func redo() -> void:
	if _redo_stack.is_empty() or solver == null:
		return
	_undo_stack.append(solver.get_solid_mask())
	solver.apply_solid_mask(_redo_stack.pop_back())
	solver.reset()
	visualizer.queue_redraw()

func reset_scene() -> void:
	if solver == null:
		return
	_push_undo()
	solver.restore_default_geometry()
	solver.reset()
	visualizer.queue_redraw()

func reset_blank_scene() -> void:
	if solver == null:
		return
	_push_undo()
	_redo_stack.clear()
	var blank := PackedByteArray()
	blank.resize(solver.width * solver.height)
	blank.fill(0)
	solver.apply_solid_mask(blank)
	solver.reset()
	camera_offset = Vector2.ZERO
	camera_zoom = 1.0
	visualizer.set_camera(camera_offset, camera_zoom)
	visualizer.queue_redraw()

func handle_input(event: InputEvent) -> bool:
	if solver == null or visualizer == null:
		return false
	if event is InputEventScreenTouch:
		return _handle_touch(event)
	if event is InputEventScreenDrag:
		return _handle_drag(event)
	if event is InputEventMouseButton:
		return _handle_mouse_button(event)
	if event is InputEventMouseMotion and (event.button_mask & MOUSE_BUTTON_MASK_LEFT) != 0:
		return _handle_mouse_motion(event)
	return false

func _handle_touch(event: InputEventScreenTouch) -> bool:
	if event.pressed:
		_active_touches[event.index] = event.position
		if _active_touches.size() == 2:
			_begin_pinch()
			_drawing = false
			_panning = true
			return true
		_begin_pointer(event.position)
	else:
		_active_touches.erase(event.index)
		if _drawing:
			_commit_geometry()
		_drawing = false
		_panning = false
		_last_grid = Vector2i(-1, -1)
	return true

func _handle_drag(event: InputEventScreenDrag) -> bool:
	_active_touches[event.index] = event.position
	if _active_touches.size() >= 2:
		_update_pinch()
		return true
	_update_pointer(event.position)
	return true

func _handle_mouse_button(event: InputEventMouseButton) -> bool:
	if event.button_index == MOUSE_BUTTON_LEFT:
		if event.pressed:
			_begin_pointer(event.position)
		else:
			if _drawing:
				_commit_geometry()
			_drawing = false
			_panning = false
		return true
	if event.button_index == MOUSE_BUTTON_WHEEL_UP and event.pressed:
		camera_zoom = clamp(camera_zoom * 1.08, 0.65, 2.5)
		visualizer.set_camera(camera_offset, camera_zoom)
		return true
	if event.button_index == MOUSE_BUTTON_WHEEL_DOWN and event.pressed:
		camera_zoom = clamp(camera_zoom / 1.08, 0.65, 2.5)
		visualizer.set_camera(camera_offset, camera_zoom)
		return true
	return false

func _handle_mouse_motion(event: InputEventMouseMotion) -> bool:
	_update_pointer(event.position)
	return true

func _begin_pointer(position: Vector2) -> void:
	_last_screen = position
	if tool == Tool.PAN:
		_panning = true
		return
	var grid := visualizer.screen_to_grid(position)
	if not _is_editable_cell(grid):
		return
	_push_undo()
	_redo_stack.clear()
	_drawing = true
	_last_grid = grid
	_paint_line(grid, grid)

func _update_pointer(position: Vector2) -> void:
	if _panning or tool == Tool.PAN:
		var delta := position - _last_screen
		camera_offset += delta
		_last_screen = position
		visualizer.set_camera(camera_offset, camera_zoom)
		return
	if not _drawing:
		return
	var grid := visualizer.screen_to_grid(position)
	if not _is_editable_cell(grid):
		return
	_paint_line(_last_grid, grid)
	_last_grid = grid

func _paint_line(from_cell: Vector2i, to_cell: Vector2i) -> void:
	var dx := abs(to_cell.x - from_cell.x)
	var sx := 1 if from_cell.x < to_cell.x else -1
	var dy := -abs(to_cell.y - from_cell.y)
	var sy := 1 if from_cell.y < to_cell.y else -1
	var err := dx + dy
	var p := from_cell
	while true:
		_paint_brush(p)
		if p == to_cell:
			break
		var e2 := 2 * err
		if e2 >= dy:
			err += dy
			p.x += sx
		if e2 <= dx:
			err += dx
			p.y += sy
	visualizer.queue_redraw()

func _paint_brush(center: Vector2i) -> void:
	for oy in range(-brush_radius, brush_radius + 1):
		for ox in range(-brush_radius, brush_radius + 1):
			if ox * ox + oy * oy > brush_radius * brush_radius + 1:
				continue
			var p := center + Vector2i(ox, oy)
			if _is_editable_cell(p):
				solver.set_solid(p.x, p.y, tool == Tool.DRAW_WALL)

func _is_editable_cell(cell: Vector2i) -> bool:
	return cell.x >= 3 and cell.x < solver.width - 3 and cell.y >= 1 and cell.y < solver.height - 1

func _commit_geometry() -> void:
	solver.reset()
	visualizer.queue_redraw()

func _push_undo() -> void:
	_undo_stack.append(solver.get_solid_mask())
	if _undo_stack.size() > 30:
		_undo_stack.pop_front()

func _begin_pinch() -> void:
	var points := _active_touches.values()
	if points.size() < 2:
		return
	_pinch_start_distance = (points[0] as Vector2).distance_to(points[1] as Vector2)
	_pinch_start_zoom = camera_zoom
	_last_screen = ((points[0] as Vector2) + (points[1] as Vector2)) * 0.5

func _update_pinch() -> void:
	var points := _active_touches.values()
	if points.size() < 2:
		return
	var a: Vector2 = points[0]
	var b: Vector2 = points[1]
	var midpoint := (a + b) * 0.5
	camera_offset += midpoint - _last_screen
	_last_screen = midpoint
	var distance := a.distance_to(b)
	if _pinch_start_distance > 1.0:
		camera_zoom = clamp(_pinch_start_zoom * distance / _pinch_start_distance, 0.65, 2.5)
	visualizer.set_camera(camera_offset, camera_zoom)
