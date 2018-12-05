static func create_mapping_table(table_path, name_column, id_column, id_unknown):
	var server_tile_file = File.new()
	
	if not server_tile_file.file_exists(table_path):
		print("Failed opening tile table")
		return
	
	result = {}
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
		var local_id = int(tileset.find_tile_by_name(name_no_quotations))
		
		# Look for variant tile if no direct hit
		# TODO: Handle variants in a good way...
		if local_id == -1:
			local_id = int(tileset.find_tile_by_name(name_no_quotations+"-0"))
		
		# Handling of unknown tile_ids
		if local_id == -1:
			local_id = id_unknown
		
		var server_id = int(values[id_column])
		print(values[id_column])
		
		result[server_id] = local_id
    return result
    
