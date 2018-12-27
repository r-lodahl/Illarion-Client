extends Control

const FILE_OP = preload("file_operations.gd")
const TABLE_LOADER = preload("table_loader.gd")

const BLOCKSIZE = 20
const BASE_ID_MASK = 0x001F
const OVERLAY_ID_MASK = 0x03E0
const SHAPE_ID_MASK = 0xFC00
const OVERLAY_MULT_FACTOR = 1000
const ID_COLUMN_TILES = 9 #10
const ID_COLUMN_OVERLAYS = 2 #3
const NAME_COLUMN_TILES = 3 #4
const NAME_COLUMN_OVERLAYS = 3 #4

var max_x = 0
var max_y = 0
var min_x = 0
var min_y = 0
var raw_map = {}
var item_strings_en = []
var item_strings_de = []
var server_tile_id_to_local_id_dic = {}

func _ready():
	#TODO: Check if saved commit id is different from current git hash	
	if true:
		var tileset = load("res://assets/tileset/tiles.res")
		TABLE_LOADER.create_mapping_table(tileset, "res://assets/tileset/tiles.tbl", NAME_COLUMN_TILES, ID_COLUMN_TILES, server_tile_id_to_local_id_dic)
		TABLE_LOADER.create_mapping_table(tileset, "res://assets/tileset/overlays.tbl", NAME_COLUMN_OVERLAYS, ID_COLUMN_OVERLAYS, server_tile_id_to_local_id_dic)
		load_raw_map()
		convert_map()
		#refresh_localization()
	get_tree().change_scene("res://world.tcsn")

# Loads all raw map files and sorts them into a map-object:
# raw_map[layer] = [mapdic, mapdic, mapdic, ...]
func load_raw_map():
	var files = FILE_OP.find_files("res://assets/map", ".tiles.txt", "Testmaps")
	
	var maps = {}
	for file in files:
		# Create the mapdic
		var mapdic = load_single_map(file)
		
		# Save the map at the correct layer-array
		if not raw_map.has(mapdic.layer): raw_map[mapdic.layer] = []
		raw_map[mapdic.layer].push_back(mapdic)
		
		# Check if the minimal or maximal values of the whole map change
		if mapdic.startx < min_x: min_x = mapdic.startx
		if mapdic.starty < min_y: min_y = mapdic.starty
		if mapdic.starty + mapdic.height > max_y: max_y = mapdic.starty + mapdic.height
		if mapdic.startx + mapdic.width > max_x: max_x = mapdic.startx + mapdic.width
	
# This function absolutly relies on the correct map scheme
# This means the sequence is comments LXYWH tiles beginning with 0 0, 0 1, etc.
func load_single_map(filepath):
	var mapfile = File.new()
	if not mapfile.file_exists(filepath):
		print("Failed opening " + filepath)
		return
	
	var mapdic = {}
	var maparray = []
		
	mapfile.open(filepath, File.READ)
	while not (mapfile.eof_reached()):
		var line = mapfile.get_line()
		
		# Skip if comment of version number
		if (line.begins_with("#") or line.begins_with("V")):
			continue
		
		# Get the layer data
		if (line.begins_with("L")):
			mapdic.layer = int(line.substr(3, line.length()-3))
			continue
		
		# Get x start value
		if (line.begins_with("X")):
			mapdic.startx = int(line.substr(3, line.length()-3))
			continue
			
		# Get y start value
		if (line.begins_with("Y")):
			mapdic.starty = int(line.substr(3, line.length()-3))
			continue
			
		# Get width of the map
		if (line.begins_with("W")):
			mapdic.width = int(line.substr(3, line.length()-3))
			continue
			
		# Get height of the map
		if (line.begins_with("H")):
			mapdic.height = int(line.substr(3, line.length()-3))
			break
		
	# Create a tile-id-array for the map
	for x in range(mapdic.width):
		maparray.append([])
		maparray[x].resize(mapdic.height)
	
	# Fill the tile-id-array
	while not (mapfile.eof_reached()):
		var line = mapfile.get_line()
		if (line == ""):
			break
		
		var values = line.split(";",false)
		maparray[int(values[0])][int(values[1])] = int(values[2])
	
	mapdic.map = maparray
	mapdic.items = {}
	mapdic.warps = {}
	
	# Open the item-file of the map
	var itemfile = File.new()
	var itempath = filepath.substr(0, filepath.length()-9)+"items.txt"
	if not mapfile.file_exists(itempath):
		print("Failed opening " + itempath)
		return mapdic
	
	itemfile.open(itempath, File.READ)
	while not (itemfile.eof_reached()):
		var line = itemfile.get_line()	
		
		if line.begins_with("#") || line == "":
			continue
		
		var values = line.split(";",false)
		
		# Save the absolute item position (x,y)
		var position = Vector2(int(values[0]), int(values[1]))
		
		if not mapdic.items.has(position): mapdic.items[position] = []
		
		var itemobj = {}
		itemobj["id"] = int(values[2])
		
		# Check if the item has any description or name
		# If yes, save their strings into item_strings and save the string_index in the item_object
		var descriptions = []
		var names = []
		for i in range(3,values.size()):
			descriptions.resize(2)
			names.resize(2)
			if values[i].begins_with("descriptionEn"):
				descriptions[0] = values[i].substr(14, values[i].length())
			elif values[i].begins_with("descriptionDe"):
				descriptions[1] = values[i].substr(14, values[i].length())
			elif values[i].begins_with("nameEn"):
				names[0] = values[i].substr(7, values[i].length())
			elif values[i].begins_with("nameDe"):
				names[1] = values[i].substr(7, values[i].length())
		
		# We do this do prevent cases where one language is null and the other one isnt
		if names[0] == null && names[1] != null || names[0] != null && names[1] == null:
			print("Warning: Missing localized name " + String(names) + " at " + itempath) 
		elif names[0] == null && names[1] != null:
			itemobj["n"] = item_strings.size()
			item_strings_en.push_back(names[0])
			item_strings_de.push_back(names[1])
		
		if descriptions[0] == null && descriptions[1] != null || descriptions[0] != null && descriptions[1] == null:
			print("Warning: Missing localized name " + String(names) + " at " + itempath) 
		elif descriptions[0] != null && descriptions[1] != null:
			itemobj["d"] = item_strings.size()
			item_strings_en.push_back(descriptions[0])
			item_strings_de.push_back(descriptions[1])
			
		mapdic.items[position].push_back(itemobj)
		
	## Save the warps if any
	var warpfile = File.new()
	var warppath = filepath.substr(0, filepath.length()-9)+"warps.txt"
	if not warpfile.file_exists(warppath):
		print("Failed opening " + warppath)
		return mapdic
	
	warpfile.open(warppath, File.READ)
	while not (warpfile.eof_reached()):
		var line = warpfile.get_line()	
		
		if line.begins_with("#") || line == "":
			continue
		
		var values = line.split(";",false)
		
		mapdic.warps[Vector2(int(values[0]),int(values[1]))] = Vector3(int(values[2]),int(values[3]),int(values[4]))
		
	return mapdic
	
func convert_map():
	# Step in BLOCKSIZE through the whole world
	# Each block will be saved as a single binary file
	for base_x in range(min_x, max_x, BLOCKSIZE):
		for base_y in range(min_y, max_y, BLOCKSIZE):
			var used_layers = []
			var used_maps = []
			var used_items = {}
			var used_warps = {}
			
			# Find maps which lie within the current block
			# Also save all layers existing in this block
			for layermaps in raw_map.values():
				for map in layermaps:
					var lefttop = Vector2(map.startx, map.starty)
					var righttop = Vector2(map.startx + map.width-1, map.starty)
					var leftbtm = Vector2(map.startx, map.starty + map.height-1)
					var rightbtm = Vector2(map.startx + map.width-1, map.starty + map.height-1)
					
					var base_mx = base_x + BLOCKSIZE-1
					var base_my = base_y + BLOCKSIZE-1
					
					if lefttop.x >= base_x && lefttop.x <= base_mx && lefttop.y >= base_y && lefttop.y <= base_my ||\
					righttop.x >= base_x && righttop.x <= base_mx && righttop.y >= base_y && righttop.y <= base_my ||\
					leftbtm.x >= base_x && leftbtm.x <= base_mx && leftbtm.y >= base_y && leftbtm.y <= base_my ||\
					rightbtm.x >= base_x && rightbtm.x <= base_mx && rightbtm.y >= base_y && rightbtm.y <= base_my:
						used_maps.push_back(map)
						used_layers.push_back(map.layer)
					
			# Skip non-existent map sections
			if used_layers.size() == 0: continue
			
			used_layers.sort()
			
			# Memory hungry, dirty, slow, but we just need to do this very rarely
			# For every tile in the block
			# Go through every map and check if this specific coordinate is in their map
			# If true: convert their tile ids to local tile ids and save them for their layer in an arry (size:layer.length)
			# If no tile found at any map for the current coordinate and layer: save 0
			var chunk_map = []
			for ix in range(base_x, base_x+BLOCKSIZE):
				for iy in range(base_y, base_y+BLOCKSIZE):
					
					var tile_ids = []
		
					for layer in used_layers:
						var layervalue = 0
						for map in used_maps:
							var x = ix - map.startx
							var y = iy - map.starty
							
							if x < 0 || y < 0 || x >= map.width || y >= map.height: continue
							layervalue = map.map[x][y]
							
							# Check if there are also items at this position, if true copy them to the chunk
							var map_position = Vector2(x,y)
							if map.items.has(map_position):
								used_items[Vector3(ix,iy,map.layer)] = map.items[map_position]
								
							# Check if there are also warps at this position, if true copy them to the chunk
							if map.warps.has(map_position):
								used_warps[Vector3(ix,iy,map.layer)] = map.warps[map_position]
						
						var server_ids = extract_server_ids(layervalue)  # Returns [base_id, overlay_id, shape_id], 0 if input 0
						
						# base_id + overlay_id: local TILESET image id
						var base_id = base_id_to_local_id(server_ids[0]) # Returns local base id. random id if has variants, 0 if input 0
						var overlay_id = overlay_id_to_local_id(server_ids[1], server_ids[2])  # Returns local shaped overlay id, 0 if input 0
						
						layervalue = overlay_id * OVERLAY_MULT_FACTOR + base_id
						
						tile_ids.push_back(layervalue)
					
					chunk_map.push_back(tile_ids)
			
			# Save the completed chunk to file
			# DBG: save as text not as binary
			
			var chunk_save = File.new()
			chunk_save.open("user://chunk_"+String(base_x)+"_"+String(base_y)+".map", File.WRITE)
			
			chunk_save.store_line(to_json([base_x,base_y]))
			chunk_save.store_line(to_json(used_layers))
			chunk_save.store_line(to_json(chunk_map))
			chunk_save.store_line(to_json(used_items))
			chunk_save.store_line(to_json(used_warps))
			
			chunk_save.close()
			
	# Save item_strings
	var string_save = File.new()
	string_save.open("user://map_strings", File.WRITE)
	
	string_save.store_line(to_json(item_strings_en))
	string_save.store_line(to_json(item_strings_de))
	
	string_save.close()
	
	#TODO: save the current git repo hash here
	var commit_save = File.new()
	commit_save.open("user://map_version", File.WRITE)
	commit_save.store_line(to_json("06272e67d89c9c14252651d9277b28fbfeb5b2b5"))
	commit_save.close()
	
func base_id_to_local_id(base_id):
	if server_tile_id_to_local_id_dic.has(base_id):
		var variant_array = server_tile_id_to_local_id_dic[base_id]
		return variant_array[randi()%variant_array.size()]
	return 0
	
func overlay_id_to_local_id(overlay_id, shape_id):
	if server_tile_id_to_local_id_dic.has(overlay_id*OVERLAY_MULT_FACTOR):
		return server_tile_id_to_local_id_dic[overlay_id*OVERLAY_MULT_FACTOR][shape_id-1]
	return 0
	
func extract_server_ids(packed_id):
	if packed_id & SHAPE_ID_MASK == 0:
		return [packed_id, 0, 0]
	return [packed_id & BASE_ID_MASK, (packed_id & OVERLAY_ID_MASK) >> 5, (packed_id & SHAPE_ID_MASK) >> 10]