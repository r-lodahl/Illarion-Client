extends Node

var _map
var _layers

var _x_per_layer = 1
var _y_per_layer = 1

var _tilemap
var _overlaymap
var _tileset

const VIS_RANGE = 10
const VIS_LAYER = 10

# TODO : Move us
var _server_tile_id_to_local_id_dic
const BASE_ID_MASK = 0x001F
const OVERLAY_ID_MASK = 0x03E0
const SHAPE_ID_MASK = 0xFC00
const OVERLAY_MULT_FACTOR = 1000
const TILE_UNKNOWN_TILE = 306

func setup_map_data(map, layers):
	print("Setting up")
	print(layers)
	_map = map
	_layers = layers
	
func setup_tile_data(tilemap, overlaymap, tileset, server_tile_id_to_local_id_dic):
	_tilemap = tilemap
	_overlaymap = overlaymap
	_tileset = tileset
	_server_tile_id_to_local_id_dic = server_tile_id_to_local_id_dic
	
func _get_layers_above(layer, limit):
	var result = []
	for i in range(_layers.find(layer)+1, _layers.size()):
		if _layers[i] - limit <= layer:
			result.push_back(_layers[i])
		else:
			break
	result.invert()
	return result
	
func _get_layers_below(layer, limit):
	var result = []
	for i in range(_layers.find(layer)):
		if _layers[i] + limit >= layer:
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
			if converted_y > single_map.starty + single_map.height || converted_y < single_map.starty ||\
			converted_x > single_map.startx + single_map.width || converted_x < single_map.startx:
				continue
			var id = single_map.map[converted_x-single_map.startx][converted_y-single_map.starty]
			if typeof(id) != TYPE_INT || id == TILE_UNKNOWN_TILE || id == null:
				continue
			return id
	return TILE_UNKNOWN_TILE
	
# TODO: use current layer + 10: if NIL: go down until not NIL
func reload_visible_tilemap(x,y,layer):
	
	var shown_layers = _get_layers_above(layer, VIS_LAYER)
	shown_layers.push_back(layer)
	shown_layers = shown_layers + _get_layers_below(layer, VIS_LAYER)
	
	for ix in range(x-VIS_RANGE, x+VIS_RANGE+1):
		for iy in range(y-VIS_RANGE, y+VIS_RANGE+1):
			var compressed_id = _tile_id_at_xy(ix,iy,shown_layers)
			var server_ids = get_real_server_ids(compressed_id)
			
			var base_id = server_tile_id_to_local_id(server_ids[0])
			if base_id != TILE_UNKNOWN_TILE:
				_tilemap.set_cell(ix,iy,base_id)

# Move them, see TODOs above
func server_tile_id_to_local_id(server_id):
	if _server_tile_id_to_local_id_dic.has(server_id):
		return _server_tile_id_to_local_id_dic[server_id]
	return TILE_UNKNOWN_TILE
						
func get_real_server_ids(packed_id):
	if packed_id & SHAPE_ID_MASK == 0:
		return [packed_id, -1]
	return [packed_id & BASE_ID_MASK, (packed_id & OVERLAY_ID_MASK) >> 5, (packed_id & SHAPE_ID_MASK) >> 10]