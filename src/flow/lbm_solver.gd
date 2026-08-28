class_name FlowLbmSolver
extends EngineeringSimulationAdapter

const Q := 9
const CX := PackedInt32Array([0, 1, 0, -1, 0, 1, -1, -1, 1])
const CY := PackedInt32Array([0, 0, 1, 0, -1, 1, 1, -1, -1])
const OPP := PackedInt32Array([0, 3, 4, 1, 2, 7, 8, 5, 6])
const W := PackedFloat64Array([4.0/9.0, 1.0/9.0, 1.0/9.0, 1.0/9.0, 1.0/9.0, 1.0/36.0, 1.0/36.0, 1.0/36.0, 1.0/36.0])

var width: int = 96
var height: int = 54
var relaxation_omega: float = 1.82
var inlet_velocity: float = 0.065
var steps_per_tick: int = 2

var _f := PackedFloat64Array()
var _next := PackedFloat64Array()
var _solid := PackedByteArray()
var _default_solid := PackedByteArray()
var _rho := PackedFloat64Array()
var _ux := PackedFloat64Array()
var _uy := PackedFloat64Array()
var _configured := false

func configure(config: Dictionary) -> void:
	width = int(config.get("width", width))
	height = int(config.get("height", height))
	relaxation_omega = float(config.get("omega", relaxation_omega))
	inlet_velocity = float(config.get("inlet_velocity", inlet_velocity))
	steps_per_tick = int(config.get("steps_per_tick", steps_per_tick))
	_allocate()
	_build_default_channel()
	reset()
	_configured = true

func is_ready() -> bool:
	return _configured

func reset() -> void:
	if _f.size() == 0:
		_allocate()
	for y in range(height):
		for x in range(width):
			var cell := _cell(x, y)
			_rho[cell] = 1.0
			_ux[cell] = inlet_velocity if not _solid[cell] else 0.0
			_uy[cell] = 0.0
			for i in range(Q):
				_f[_slot(cell, i)] = _equilibrium(i, 1.0, _ux[cell], 0.0)
	_next = _f.duplicate()

func step(_delta: float) -> void:
	if not _configured:
		return
	for _n in range(max(1, steps_per_tick)):
		_collide_and_stream()
		_apply_inlet_outlet()
		_reconstruct_macros()

func set_solid(x: int, y: int, value: bool) -> void:
	if x < 0 or x >= width or y < 0 or y >= height:
		return
	_solid[_cell(x, y)] = 1 if value else 0

func get_solid_mask() -> PackedByteArray:
	return _solid.duplicate()

func apply_solid_mask(mask: PackedByteArray) -> void:
	if mask.size() != width * height:
		return
	_solid = mask.duplicate()
	_enforce_channel_edges()

func restore_default_geometry() -> void:
	if _default_solid.size() == width * height:
		_solid = _default_solid.duplicate()
	else:
		_build_default_channel()

func get_metrics() -> Dictionary:
	var outlet_speed := 0.0
	var outlet_density := 0.0
	var inlet_density := 0.0
	var samples := 0
	var inlet_samples := 0
	var kinetic := 0.0
	var vorticity_total := 0.0
	var vorticity_samples := 0
	var solid_cells := 0
	var base_solid_cells := 0

	for y in range(1, height - 1):
		var outlet := _cell(width - 2, y)
		if not _solid[outlet]:
			outlet_speed += sqrt(_ux[outlet] * _ux[outlet] + _uy[outlet] * _uy[outlet])
			outlet_density += _rho[outlet]
			samples += 1
		var inlet := _cell(1, y)
		if not _solid[inlet]:
			inlet_density += _rho[inlet]
			inlet_samples += 1

	for y in range(height):
		for x in range(width):
			var c := _cell(x, y)
			if _solid[c]:
				solid_cells += 1
				continue
			kinetic += _ux[c] * _ux[c] + _uy[c] * _uy[c]
			if x > 0 and x < width - 1 and y > 0 and y < height - 1:
				var dv_dx := (_uy[_cell(x + 1, y)] - _uy[_cell(x - 1, y)]) * 0.5
				var du_dy := (_ux[_cell(x, y + 1)] - _ux[_cell(x, y - 1)]) * 0.5
				vorticity_total += abs(dv_dx - du_dy)
				vorticity_samples += 1

	for value in _default_solid:
		if value == 1:
			base_solid_cells += 1

	var mean_inlet_density := inlet_density / max(1, inlet_samples)
	var mean_outlet_density := outlet_density / max(1, samples)
	return {
		"mean_outlet_speed": outlet_speed / max(1, samples),
		"mean_inlet_density": mean_inlet_density,
		"mean_outlet_density": mean_outlet_density,
		"pressure_loss_proxy": abs(mean_inlet_density - mean_outlet_density),
		"mean_vorticity_proxy": vorticity_total / max(1, vorticity_samples),
		"solid_cells": solid_cells,
		"base_solid_cells": base_solid_cells,
		"kinetic_energy_proxy": kinetic,
		"grid_width": width,
		"grid_height": height,
		"solver": "D2Q9 LBM CPU prototype"
	}

func get_visualization_data() -> Dictionary:
	return {
		"width": width,
		"height": height,
		"density": _rho,
		"velocity_x": _ux,
		"velocity_y": _uy,
		"solid": _solid
	}

func _allocate() -> void:
	var cells := width * height
	_f.resize(cells * Q)
	_next.resize(cells * Q)
	_solid.resize(cells)
	_default_solid.resize(cells)
	_rho.resize(cells)
	_ux.resize(cells)
	_uy.resize(cells)
	_solid.fill(0)

func _build_default_channel() -> void:
	_solid.fill(0)
	_enforce_channel_edges()
	var cx := int(width * 0.52)
	var cy := int(height * 0.50)
	var radius := max(3, int(min(width, height) * 0.11))
	for y in range(height):
		for x in range(width):
			var dx := x - cx
			var dy := y - cy
			if dx * dx + dy * dy <= radius * radius:
				_solid[_cell(x, y)] = 1
	_default_solid = _solid.duplicate()

func _enforce_channel_edges() -> void:
	for x in range(width):
		_solid[_cell(x, 0)] = 1
		_solid[_cell(x, height - 1)] = 1
	for y in range(1, height - 1):
		_solid[_cell(1, y)] = 0
		_solid[_cell(width - 2, y)] = 0

func _collide_and_stream() -> void:
	_next.fill(0.0)
	for y in range(height):
		for x in range(width):
			var cell := _cell(x, y)
			if _solid[cell]:
				for i in range(Q):
					_next[_slot(cell, OPP[i])] += _f[_slot(cell, i)]
				continue
			var rho := 0.0
			var ux := 0.0
			var uy := 0.0
			for i in range(Q):
				var fi := _f[_slot(cell, i)]
				rho += fi
				ux += fi * CX[i]
				uy += fi * CY[i]
			if rho <= 0.000001:
				rho = 1.0
			ux /= rho
			uy /= rho
			for i in range(Q):
				var fi := _f[_slot(cell, i)]
				var feq := _equilibrium(i, rho, ux, uy)
				var post := fi - relaxation_omega * (fi - feq)
				var nx := x + CX[i]
				var ny := y + CY[i]
				if nx < 0 or nx >= width or ny < 0 or ny >= height:
					_next[_slot(cell, OPP[i])] += post
				elif _solid[_cell(nx, ny)]:
					_next[_slot(cell, OPP[i])] += post
				else:
					_next[_slot(_cell(nx, ny), i)] += post
	var swap := _f
	_f = _next
	_next = swap

func _apply_inlet_outlet() -> void:
	for y in range(1, height - 1):
		var inlet := _cell(1, y)
		if not _solid[inlet]:
			for i in range(Q):
				_f[_slot(inlet, i)] = _equilibrium(i, 1.0, inlet_velocity, 0.0)
		var outlet := _cell(width - 2, y)
		var upstream := _cell(width - 3, y)
		if not _solid[outlet] and not _solid[upstream]:
			for i in range(Q):
				_f[_slot(outlet, i)] = _f[_slot(upstream, i)]

func _reconstruct_macros() -> void:
	for y in range(height):
		for x in range(width):
			var cell := _cell(x, y)
			if _solid[cell]:
				_rho[cell] = 1.0
				_ux[cell] = 0.0
				_uy[cell] = 0.0
				continue
			var rho := 0.0
			var ux := 0.0
			var uy := 0.0
			for i in range(Q):
				var fi := _f[_slot(cell, i)]
				rho += fi
				ux += fi * CX[i]
				uy += fi * CY[i]
			if rho <= 0.000001:
				rho = 1.0
			_rho[cell] = rho
			_ux[cell] = ux / rho
			_uy[cell] = uy / rho

func _equilibrium(i: int, rho: float, ux: float, uy: float) -> float:
	var cu := 3.0 * (CX[i] * ux + CY[i] * uy)
	var u2 := ux * ux + uy * uy
	return W[i] * rho * (1.0 + cu + 0.5 * cu * cu - 1.5 * u2)

func _cell(x: int, y: int) -> int:
	return y * width + x

func _slot(cell: int, direction: int) -> int:
	return cell * Q + direction
