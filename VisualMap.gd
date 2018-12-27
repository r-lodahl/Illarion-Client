extends Node

# visible tile shift per layer upwards
const _x_per_layer = 1
const _y_per_layer = 1

# Current map-center
var _x
var _y
var _chunk_x
var _chunk_y
var _layer

# tilemap
var _tilemap
var _overlaymap
var _tileset

# datamap
var _used_layers
var _map
#########
#.0.1.2.#
#.3.4.5.#
#.6.7.8.#
#########

# Visible tile radius
const VIS_RANGE = 20
const VIS_LAYER = 10
const OVERLAY_MULT_FACTOR = 1000
const BLOCKSIZE = 20
	
func setup(tilemap, overlaymap, server_tile_id_to_local_id_dic):
	_tilemap = tilemap
	_overlaymap = overlaymap
	
	_map = []
	_map.resize(9)

func _calculate_used_layers():
	_used_layers = []
	for chunk in map:
		for layer in map.layers:
			if (not _used_layers.has(layer)) && layer <= _layer+VIS_RANGE && layer >= _layer-VIS_RANGE:
				_used_layers.push_back(layer)
	_used_layers.sort()

func reload_map_at(x,y,layer):
	_x = x
	_y = y
	_layer = layer
	_reload_all_chunks()
	_calculate_used_layers()
	_reload_visible_tilemap()

func _reload_all_chunks():
	_chunk_x = floor(_x / BLOCKSIZE) * BLOCKSIZE
	_chunk_y = floor(_y / BLOCKSIZE) * BLOCKSIZE
	
	map[0] = _load_map_file("user://chunk_"+String(_chunk_x-BLOCKSIZE)+"_"+String(_chunk_y-BLOCKSIZE)+".map")
	map[1] = _load_map_file("user://chunk_"+String(_chunk_x)+"_"+String(_chunk_y-BLOCKSIZE)+".map")
	map[2] = _load_map_file("user://chunk_"+String(_chunk_x+BLOCKSIZE)+"_"+String(_chunk_y-BLOCKSIZE)+".map")
	map[3] = _load_map_file("user://chunk_"+String(_chunk_x-BLOCKSIZE)+"_"+String(_chunk_y)+".map")
	map[4] = _load_map_file("user://chunk_"+String(_chunk_x)+"_"+String(_chunk_y)+".map")
	map[5] = _load_map_file("user://chunk_"+String(_chunk_x+BLOCKSIZE)+"_"+String(_chunk_y)+".map")
	map[6] = _load_map_file("user://chunk_"+String(_chunk_x-BLOCKSIZE)+"_"+String(_chunk_y+BLOCKSIZE)+".map")
	map[7] = _load_map_file("user://chunk_"+String(_chunk_x)+"_"+String(_chunk_y+BLOCKSIZE)+".map")
	map[8] = _load_map_file("user://chunk_"+String(_chunk_x+BLOCKSIZE)+"_"+String(_chunk_y+BLOCKSIZE)+".map")

func _load_map_file(filepath):
	var mapfile = File.new()
	if not mapfile.file_exists(filepath):
		print("Failed opening " + filepath)
		return null
	
	var chunk = {}
	mapfile.open(filepath, File.READ)
	chunk["start"] = from_json(mapfile.get_line())
	chunk["layers"] = from_json(mapfile.get_line())
	chunk["tiles"] = from_json(mapfile.get_line())
	chunk["items"] = from_json(mapfile.get_line())
	chunk["warps"] = from_json(mapfile.get_line())
	mapfile.close()
	
func _mapcenter_was_moved(tx, ty):
	# TODO: If movement changes position mod BLOCKSIZE:
	# unload no longer needed old maps
	# load needed new maps 
	
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
	for ix in range(_x-VIS_RANGE, _x+VIS_RANGE+1):
		for iy in range(_y-VIS_RANGE, _y+VIS_RANGE+1):
			_reload_tilemap_tile(ix,iy)	

func _reload_tilemap_tile(ix,iy):
	var ids = _tile_ids_at_xy(ix,iy)
	
	if ids[0] != 0:
		_tilemap.set_cell(iy,-ix,ids[0])
		
	if ids[1] != 0:
		_overlaymap.set_cell(iy,-ix,ids[1])

# For given x,y ON the tilemap returns the tile-id at the shifted layer-xy (aka the xy
# that is actually visibile on the tilemap xy) for a given layerbase
# It is expected that argument layers is ordered with top layer at position 0
func _tile_ids_at_xy(x,y):
	for layer in _used_layers:
		for chunk in _map:
			if not chunk.layers.has(layer): continue
			
			var converted_x = x - _x_per_layer * layer - chunk.start[0]
			var converted_y = y - _y_per_layer * layer - chunk.start[1]
			
			if converted_x < 0 || converted_y < 0 || converted_x >= BLOCKSIZE || converted_y >= BLOCKSIZE: continue
			
			var id = chunk.map[converted_x + converted_y + BLOCKSIZE]
			
			if id == 0: continue
			
			return [floor(id/1000), id%1000]
	return [0,0]