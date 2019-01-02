static func create_mapping_table(tileset, table_path, name_column, id_column, out_result_dic):
	var server_tile_file = File.new()
	
	if not server_tile_file.file_exists(table_path):
		print("Failed opening tile table")
		return
	
	server_tile_file.open(table_path, File.READ)
	while not (server_tile_file.eof_reached()):
		var line = server_tile_file.get_line()
		
		if line == "":
			break
			
		if line.begins_with("#"):
			continue
		
		var values = line.split(",",false)
	
		var name_no_quotations = values[name_column].substr(1, values[name_column].length()-2)
		
		# Direct hit?
		var local_id = [int(tileset.find_tile_by_name(name_no_quotations))]
		
		# Look for variant tile if no direct hit
		# TODO: Handle variants in a good way...
		if local_id[0] == -1:
			var ids = []
			
			var variant_id = 0
			local_id = int(tileset.find_tile_by_name(name_no_quotations+"-"+String(variant_id)))
			while local_id != -1:
				ids.push_back(local_id)
				variant_id += 1
				local_id = int(tileset.find_tile_by_name(name_no_quotations+"-"+String(variant_id)))
			local_id = ids
			
		# Handling of unknown tile_ids
		if local_id.size() == 0:
			local_id = [0]
		
		var server_id = int(values[id_column])
		
		out_result_dic[server_id] = local_id
		
static func create_item_table(table_path, local_item_dic):
	var id_column = 2
	var name_column = 3
	var offset_x_column = 6
	var offset_y_column = 7
	var color_r_column = 23
	var color_g_column = 24
	var color_b_column = 25
	var color_a_column = 26
	var level_column = 20
	var light_column = 22
	var light_opacity_column = 16
	var shadow_offset_column = 13
	var facing_column = 10
	var frames_column = 4
	var is_animated_column = 5
	var is_movable_column = 11
	var is_blocking_column = 17
	var special_item_flag_column = 14
	var animation_speed_column = 8
	var min_max_scaling_column = 15
	
	var item_file = File.new()
	
	if not item_file.file_exists(table_path):
		print("Failed opening item table")
		return
	
	item_file.open(table_path, File.READ)
	
	var item_dic = {}
	while not (item_file.eof_reached()):
		var line = item_file.get_line()
		
		if line == "":
			break
			
		if line.begins_with("/"):
			continue
		
		var values = line.split(",",false)
		var name_no_quotes = values[name_column].substr(1, values[name_column].length()-2)
		
		var spriteset_id
		var rect_array
		var local_item = local_item_dic[name_no_quotes]
		if local_item == null:
			item_rect = [[0,0,0,0]]
			spriteset_id = 0
			print("Failed finding item " + name_no_quotes)
		
		var item = {}
		item.image = spriteset_id
		item.rects = rect_array
		item.offset = [values[offset_x_column], values[offset_y_column]]
		item.color = Color(values[color_r_column],values[color_g_column],values[color_b_column],values[color_a_column])
		item.animated = values[is_animated_column] == 1
		
		if (item.animated):
			item.speed = values[animation_speed_column]
			
		item_dic[values[id_column]] = item
	return item_dic
		
		
		
	
		
		
		
    
