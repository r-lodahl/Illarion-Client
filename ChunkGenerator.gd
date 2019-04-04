extends Node

var tile_width
var tile_height
var tile_texture
var tile_set

var chunk_size

var vis_range
var x_shift_per_layer
var y_shift_per_layer
var overlay_mult_factor

func _ready():
	pass

func generate_chunk(map, chunk_id, main_layer, top_visible):
	var thread = Thread.new()
	thread.start(self, "generate_texture", [map, chunk_id, main_layer, top_visible])
	
func generate_texture(thread_data):
	var map = thread_data[0]
	var id = thread_data[1]
	var main_layer = thread_data[2]
	var top_visible = thread_data[3]
	
	var used_layers = _calculate_used_layers(map, main_layer)
	
	var texture = Image.new()
	texture.create(chunk_size*tile_width, chunk_size*tile_height, false, Image.FORMAT_RGBA8)
	texture.fill(Color(0,0,0,0))
	
	for chunk in map:
		for x in range(chunk.start[0],chunk.start[0]+chunk_size):
			for y in range(chunk.start[1],chunk.start[1]+chunk_size):
				var ids = _tile_ids_at_xy(x,y,map,used_layers)
				
				if ids[0] != 0:
					var rect = tile_set.tile_get_region(ids[0])
					
					tile_texture.lock()
					
					for w in tile_width:
						for h in tile_height:
							
							var converted_x = x - chunk.start[0]
							var converted_y = y - chunk.start[1]
							
							texture.set_pixel(\
								converted_x * (tile_width/2.0) + converted_y * (tile_width/2.0) + w,\
								(texture.get_height()/2.0) + (tile_height/2.0) * converted_x - (tile_height/2.0) * converted_y + h,\
								tile_texture.get_data().get_pixel(rect.position.x + w, rect.position.y + h)\
							)
							
					tile_texture.unlock()
	return texture
	
func _calculate_used_layers(map, layer):
	var used_layers = []
	for chunk in map:
		if chunk == null: continue
		for layer in chunk.layers:
			if (not used_layers.has(layer)) && layer <= layer+vis_range && layer >= layer-vis_range:
				used_layers.push_back(layer)
	used_layers.sort()
	return used_layers
	
# For given x,y ON the tilemap returns the tile-id at the shifted layer-xy (aka the xy
# that is actually visibile on the tilemap xy) for a given layerbase
# It is expected that argument layers is ordered with top layer at position 0
func _tile_ids_at_xy(x,y,map,used_layers):
	for layer in used_layers:
		for chunk in map:
			if chunk == null: continue
			
			var layer_idx = chunk.layers.find(layer)
			if layer_idx == -1: continue
			
			var converted_x = int(x - x_shift_per_layer * layer - chunk.start[0])
			var converted_y = int(y - y_shift_per_layer * layer - chunk.start[1])
			
			if converted_x < 0 || converted_y < 0 || converted_x >= chunk_size || converted_y >= chunk_size: continue
			
			var id = int(chunk.tiles[converted_y + converted_x * chunk_size][layer_idx])
			
			if id == 0: continue
			
			return [id%overlay_mult_factor, floor(id/overlay_mult_factor)]
	return [0,0]