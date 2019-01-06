extends Node

# visible tile shift per layer upwards
const _x_per_layer = 1
const _y_per_layer = 1
const _tilesize_x = 76
const _tilesize_y = 38

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

# Other data
var _itemdic

# Visible tile radius
const VIS_RANGE = 20
const VIS_LAYER = 10
const OVERLAY_MULT_FACTOR = 1000
const BLOCKSIZE = 20.0
	
func setup(tilemap, overlaymap, itemdic):
	_tilemap = tilemap
	_overlaymap = overlaymap
	_itemdic = itemdic
	
	_map = []
	_map.resize(9)

func _calculate_used_layers():
	_used_layers = []
	for chunk in _map:
		if chunk == null: continue
		for layer in chunk.layers:
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
	
	_load_chunks_at([0,1,2,3,4,5,6,7,8])

func _load_chunks_at(chunk_array):
	for i in chunk_array:
		_map[i] = _load_map_file("user://chunk_"+String(_chunk_x+(((i%3)-1)*BLOCKSIZE))+\
		"_"+String(_chunk_y+((floor(i/3)-1)*BLOCKSIZE))+".map")

func _load_map_file(filepath):
	var mapfile = File.new()
	if not mapfile.file_exists(filepath):
		print("Failed opening " + filepath)
		return null
	
	var chunk = {}
	mapfile.open(filepath, File.READ)
	
	chunk["start"] = mapfile.get_var()
	chunk["layers"] = mapfile.get_var()
	chunk["tiles"] = mapfile.get_var()
	chunk["items"] = mapfile.get_var()
	chunk["warps"] = mapfile.get_var()
	
	mapfile.close()
	return chunk
	
func _mapcenter_was_moved(tx, ty):
	# TODO: If movement changes position mod BLOCKSIZE:
	# unload no longer needed old maps
	# load needed new maps 
	_x += tx
	_y += ty
	
	var chunk_x = floor(_x / BLOCKSIZE) * BLOCKSIZE
	var chunk_y = floor(_y / BLOCKSIZE) * BLOCKSIZE
	
	# Check 8-way-movement
	if chunk_x != _chunk_x || chunk_y != _chunk_y:
		var new_chunks
		if chunk_x < _chunk_x && chunk_y < _chunk_y:
			_map[8] = _map[4]
			_map[7] = _map[3]
			_map[5] = _map[1]
			_map[4] = _map[0]
			new_chunks = [0,1,2,3,6]
		elif chunk_x < _chunk_x && chunk_y > _chunk_y:
			_map[2] = _map[4]
			_map[1] = _map[3]
			_map[5] = _map[7]
			_map[4] = _map[6]
			new_chunks = [0,3,6,7,8]
		elif chunk_x > _chunk_x && chunk_y < chunk_y:
			_map[6] = _map[4]
			_map[7] = _map[5]
			_map[3] = _map[1]
			_map[4] = _map[2]
			new_chunks = [0,1,2,5,8]
		elif chunk_x > _chunk_x && chunk_y > _chunk_y:
			_map[0] = _map[4]
			_map[1] = _map[5]
			_map[3] = _map[7]
			_map[4] = _map[8]
			new_chunks = [2,5,6,7,8]
		elif chunk_x > _chunk_x:
			for i in range(0,8,3):
				_map[i] = _map[i+1]
				_map[i+1] = _map[i+2]
			new_chunks = [2,5,8]
		elif chunk_x < _chunk_x:
			for i in range(0,8,3):
				_map[i+2] = _map[i+1]
				_map[i+1] = _map[i] 
			new_chunks = [0,3,6]
		elif chunk_y < _chunk_y:
			for i in range(5,-1,-1):
				_map[i+3] = _map[i]
			new_chunks = [0,1,2]
		elif chunk_y > _chunk_y:
			for i in range(3,9):
				_map[i-3] = _map[i]
			new_chunks = [6,7,8]
		_chunk_x = chunk_x
		_chunk_y = chunk_y
		_load_chunks_at(new_chunks)
		
	if tx > 0: _load_tilemapstripe_add_x(tx)
	elif tx < 0: _load_tilemapstripe_sub_x(tx)
	
	if ty > 0: _load_tilemapstripe_add_y(ty)
	elif ty < 0: _load_tilemapstripe_sub_y(ty)

func _load_tilemapstripe_add_x(translated_x):
	for ix in range(_x + VIS_RANGE - translated_x + 1, _x + VIS_RANGE+1):
		for iy in range(_y - VIS_RANGE, _y + VIS_RANGE + 1): 
			_reload_tilemap_tile(ix,iy)	
			_reload_items(ix,iy)
			
func _load_tilemapstripe_sub_x(translated_x):
	for ix in range(_x - VIS_RANGE, _x - VIS_RANGE - translated_x):
		for iy in range(_y - VIS_RANGE, _y + VIS_RANGE + 1): 
			_reload_tilemap_tile(ix,iy)	
			_reload_items(ix,iy)
			
func _load_tilemapstripe_add_y(translated_y):
	for ix in range(_x - VIS_RANGE, _x + VIS_RANGE+1):
		for iy in range(_y + VIS_RANGE - translated_y + 1, _y + VIS_RANGE+1): 
			_reload_tilemap_tile(ix,iy)	
			_reload_items(ix,iy)
			
func _load_tilemapstripe_sub_y(translated_y):
	for ix in range(_x - VIS_RANGE, _x + VIS_RANGE + 1):
		for iy in range(_y - VIS_RANGE, _y - VIS_RANGE - translated_y): 
			_reload_tilemap_tile(ix,iy)	
			_reload_items(ix,iy)
	
# TODO: use current layer + 10: if NIL: go down until not NIL
func _reload_visible_tilemap():
	for ix in range(_x-VIS_RANGE, _x+VIS_RANGE+1):
		for iy in range(_y-VIS_RANGE, _y+VIS_RANGE+1):
			_reload_tilemap_tile(ix,iy)	
			_reload_items(ix,iy)

func _reload_items(ix,iy):
	var items = _items_at_xy(ix,iy)
	
	for item in items:
		var sprite_base = _itemdic[item.id]
		
		if sprite_base == null:
			continue
			
		var sprite = Sprite.new()
		sprite.texture = sprite_base.res[0]
		
		var position = _overlaymap.map_to_world(Vector2(iy,-ix))
		position.x = position.x + sprite_base.offset[0]
		position.y = position.y + sprite_base.offset[1]
		
		sprite.global_position = position
		_overlaymap.add_child(sprite)
	
	# TODO: Find a way to mark a field as already drawn with the node OR make sure that we clear all sprites on a tile
	# before redrawing them. 

#TODO: Merge with tile ids at xy
func _items_at_xy(x,y):
	for layer in _used_layers:
		for chunk in _map:
			if chunk == null: continue
			
			var layer_idx = chunk.layers.find(layer)
			if layer_idx == -1: continue
			
			var converted_x = int(x - _x_per_layer * layer - chunk.start[0])
			var converted_y = int(y - _y_per_layer * layer - chunk.start[1])
			
			if converted_x < 0 || converted_y < 0 || converted_x >= BLOCKSIZE || converted_y >= BLOCKSIZE: continue
			
			if x == 1 && y == 0 && layer == 0:
				print("BREAK")
			
			
			var position = Vector3(converted_x, converted_y, layer)
			if chunk.items.has(position): 
				return chunk.items[position]
	
	return []
			
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
			if chunk == null: continue
			
			var layer_idx = chunk.layers.find(layer)
			if layer_idx == -1: continue
			
			var converted_x = int(x - _x_per_layer * layer - chunk.start[0])
			var converted_y = int(y - _y_per_layer * layer - chunk.start[1])
			
			if converted_x < 0 || converted_y < 0 || converted_x >= BLOCKSIZE || converted_y >= BLOCKSIZE: continue
			
			var id = int(chunk.tiles[converted_y + converted_x * BLOCKSIZE][layer_idx])
			
			if id == 0: continue
			
			return [id%OVERLAY_MULT_FACTOR, floor(id/OVERLAY_MULT_FACTOR)]
	return [0,0]