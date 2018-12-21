extends Node

# TODO: Needs to load chunks now.. do that with file_loader?

# map an layers
var _map
var _layers

# visible tile shift per layer upwards
const _x_per_layer = 1
const _y_per_layer = 1

# Current map-center
var _x
var _y
var _layer

# tilemap
var _tilemap
var _overlaymap
var _tileset

# Visible tile radius
const VIS_RANGE = 20
const VIS_LAYER = 10
const OVERLAY_MULT_FACTOR = 1000

func setup_map_data(map, layers):
	print("Setting up")
	print(layers)
	_map = map
	_layers = layers
	
func setup_tile_data(tilemap, overlaymap, server_tile_id_to_local_id_dic):
	_tilemap = tilemap
	_overlaymap = overlaymap
	_server_tile_id_to_local_id_dic = server_tile_id_to_local_id_dic

func _get_layers_around(limit):
	var shown_layers = _get_layers_above(VIS_LAYER)
	shown_layers.push_back(_layer)
	shown_layers = shown_layers + _get_layers_below(VIS_LAYER)
	return shown_layers

func _get_layers_above(limit):
	var result = []
	for i in range(_layers.find(_layer)+1, _layers.size()):
		if _layers[i] - limit <= _layer:
			result.push_back(_layers[i])
		else:
			break
	result.invert()
	return result
	
func _get_layers_below(limit):
	var result = []
	for i in range(_layers.find(_layer)):
		if _layers[i] + limit >= _layer:
			result.push_back(_layers[i])
	result.invert()
	return result
	
# For given x,y ON the tilemap returns the tile-id at the shifted layer-xy (aka the xy
# that is actually visibile on the tilemap xy) for a given layerbase
# It is expected that argument layers is ordered with top layer at position 0
func _tile_id_at_xy(x,y,layers):
	for layer in layers:
		for single_map in _map[layer]:
			var converted_x = x + _x_per_layer * layer
			var converted_y = y + _y_per_layer * layer
			if converted_y > single_map.starty + single_map.height-1 || converted_y < single_map.starty ||\
			converted_x > single_map.startx + single_map.width-1 || converted_x < single_map.startx:
				continue
			var id = single_map.map[converted_x-single_map.startx][converted_y-single_map.starty]
			if id == 0:
				continue
			return id
	return 0

func set_mapcenter(x,y,layer):
	_x = x
	_y = y
	_layer = layer
	_reload_visible_tilemap()
	
func _mapcenter_was_moved(tx, ty):
	_x += tx
	_y += ty
	
	var layers = _get_layers_around(10)
	
	if tx > 0: _load_tilemapstripe_add_x(tx, layers)
	elif tx < 0: _load_tilemapstripe_sub_x(tx, layers)
	
	if ty > 0: _load_tilemapstripe_add_y(ty, layers)
	elif ty < 0: _load_tilemapstripe_sub_y(ty, layers)

func _load_tilemapstripe_add_x(translated_x, layers):
	for ix in range(_x + VIS_RANGE - translated_x + 1, _x + VIS_RANGE+1):
		for iy in range(_y - VIS_RANGE, _y + VIS_RANGE + 1): 
			_reload_tilemap_tile(ix,iy,layers)	
			
func _load_tilemapstripe_sub_x(translated_x, layers):
	for ix in range(_x - VIS_RANGE, _x - VIS_RANGE - translated_x):
		for iy in range(_y - VIS_RANGE, _y + VIS_RANGE + 1): 
			_reload_tilemap_tile(ix,iy,layers)	
			
func _load_tilemapstripe_add_y(translated_y, layers):
	for ix in range(_x - VIS_RANGE, _x + VIS_RANGE+1):
		for iy in range(_y + VIS_RANGE - translated_y + 1, _y + VIS_RANGE+1): 
			_reload_tilemap_tile(ix,iy,layers)	
			
func _load_tilemapstripe_sub_y(translated_y, layers):
	for ix in range(_x - VIS_RANGE, _x + VIS_RANGE + 1):
		for iy in range(_y - VIS_RANGE, _y - VIS_RANGE - translated_y): 
			_reload_tilemap_tile(ix,iy,layers)	
	
# TODO: use current layer + 10: if NIL: go down until not NIL
func _reload_visible_tilemap():
	
	var shown_layers = _get_layers_above(VIS_LAYER)
	shown_layers.push_back(_layer)
	shown_layers = shown_layers + _get_layers_below(VIS_LAYER)
	
	for ix in range(_x-VIS_RANGE, _x+VIS_RANGE+1):
		for iy in range(_y-VIS_RANGE, _y+VIS_RANGE+1):
			_reload_tilemap_tile(ix,iy,shown_layers)	

func _reload_tilemap_tile(ix,iy,shown_layers):
	var compressed_id = _tile_id_at_xy(ix,iy,shown_layers)
	var server_ids = get_real_server_ids(compressed_id)
	
	var base_ids = server_tile_id_to_local_id(server_ids[0])
	if base_ids[0] != TILE_UNKNOWN_TILE:
		_tilemap.set_cell(iy,-ix,base_ids[randi()%base_ids.size()])
		
	if server_ids[1] != -1:
		var overlay_ids = server_tile_id_to_local_id(server_ids[1] * OVERLAY_MULT_FACTOR)
		if overlay_ids[0] != TILE_UNKNOWN_TILE:
			_overlaymap.set_cell(iy,-ix,overlay_ids[server_ids[2]-1])

