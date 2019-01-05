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
		
static func create_item_table(table_path, local_items):
	var id_column = 2
	var name_column = 3
	
	## First: Load the server item names
	var item_file = File.new()
	
	if not item_file.file_exists(table_path):
		print("Failed opening tile table")
		return
	
	var server_names = []
	item_file.open(table_path, File.READ)
	while not (item_file.eof_reached()):
		var line = item_file.get_line()
		
		if line == "":
			break
			
		if line.begins_with("/"):
			continue
		
		var values = line.split(",",false)
		var cleaned_name = values[name_column].substr(1, values[name_column].length()-2)
		var path_idx = cleaned_name.rfind("/")
		
		if path_idx > -1:
			cleaned_name = cleaned_name.substr(path_idx+1, cleaned_name.length() - path_idx)
		
		server_names.push_back(cleaned_name)
	
	## Second: Get the local names and sprites and sort them according to the server files
	var local_item_dic = {}
	for itempath in local_items:
		var itemname = itempath.get_file().get_basename().strip_edges()
		var variation = -1
		var variant_name
		
		if not server_names.has(itemname):	
			var variation_idx = itemname.rfind("-")
		
			if variation_idx < 0:
				print("Could not find in server item names: " + itemname)
				continue
		
			var variation_candidate = itemname.substr(variation_idx+1,itemname.length()-variation_idx)
			itemname = itemname.substr(0,variation_idx)
			
			if not variation_candidate.is_valid_integer() or not server_names.has(itemname):
				print("Could not find in server item names: " + itemname)
				continue
		
			variation = int(variation_candidate)
			variant_name = itemname
		
		if not local_item_dic.has(itemname):
			local_item_dic[itemname] = []
			
		if variation == -1:
			local_item_dic[itemname].push_back(load(itempath))#itemname)
		else:
			if local_item_dic[itemname].size() <= variation:
				local_item_dic[itemname].resize(variation+1)
			local_item_dic[itemname].insert(variation,load(itempath)) #variant_name)
	
	## Third: Load item data from item table
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
	
	var item_dic = {}
	var unknown_items = []
	item_file.seek(0) #Put cursor back to beginning of the file
	while not (item_file.eof_reached()):
		var line = item_file.get_line()
		
		if line == "":
			break
			
		if line.begins_with("/"):
			continue
		
		var values = line.split(",",false)
		var cleaned_name = values[name_column].substr(1, values[name_column].length()-2)
		var path_idx = cleaned_name.rfind("/")
		
		if path_idx > -1:
			cleaned_name = cleaned_name.substr(path_idx+1, cleaned_name.length() - path_idx)
		
		if not local_item_dic.has(cleaned_name):
			unknown_items.push_back(values[id_column])
			print("Failed finding item " + cleaned_name)
			continue		
		
		var item = {}
		item.res = local_item_dic[cleaned_name]
		item.offset = [int(values[offset_x_column]), int(values[offset_y_column])]
		item.color = Color(float(values[color_r_column])/255,float(values[color_g_column])/255,float(values[color_b_column])/255,float(values[color_a_column])/255)

		if (int(values[is_animated_column]) == 1):
			item.speed = int(values[animation_speed_column])
			
		item_dic[int(values[id_column])] = item
		
	for item_id in unknown_items:
		item_dic[item_id] = item_dic[0]
	
	item_file.close()
	
	return item_dic