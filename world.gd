extends Node2D

const title = "Illarion Client v2.0"

func _process(delta):
	OS.set_window_title(title + " | FPS: " + str(Engine.get_frames_per_second()))

###########################
# DYNAMICALLY MAP LOADING #
###########################
var server_tile_id_to_local_id_dic = {}
var map = {}
var tilemaps = {}

const BASE_ID_MASK = 0x001F
const OVERLAY_ID_MASK = 0x03E0
const SHAPE_ID_MASK = 0xFC00
const OVERLAY_MULT_FACTOR = 1000
const TILE_UNKNOWN_TILE = 306
const ID_COLUMN_TILES = 9 #10
const ID_COLUMN_OVERLAYS = 2 #3
const NAME_COLUMN_TILES = 3 #4
const NAME_COLUMN_OVERLAYS = 3 #4

func _ready():
	create_mapping_table("res://assets/tileset/tiles.tbl", NAME_COLUMN_TILES, ID_COLUMN_TILES)
	create_mapping_table("res://assets/tileset/overlays.tbl", NAME_COLUMN_OVERLAYS, ID_COLUMN_OVERLAYS)
	load_maps()

func create_mapping_table(table_path, name_column, id_column):
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
		var local_id = int(tile_set.find_tile_by_name(name_no_quotations))
		
		# Look for variant tile if no direct hit
		# TODO: Handle variants in a good way...
		if local_id == -1:
			local_id = int(tile_set.find_tile_by_name(name_no_quotations+"-0"))
		
		# Handling of unknown tile_ids
		if local_id == -1:
			local_id = TILE_UNKNOWN_TILE
		
		var server_id = int(values[id_column])
		print(values[id_column])
		
		server_tile_id_to_local_id_dic[server_id] = local_id

func load_maps():
	var files = find_files("res://assets/map", ".tiles.txt", "Testmaps")

	for file in files:
		var mapdic = load_single_map(file)
		
		if not map.has(mapdic.layer):
			map[madic.layer] = []

		map[mapdic.layer].push_back(mapdic)

	# ensure the correct layer sequence
	for layer in mapdic.keys.sort():
		var node = TileMap.new()
		node.cell_size = Vector3(76,37,0)
		node.cell_y_sort = true
		node.mode = MODE_ISOMETRIC
		node.tile_set = load("res://assets/tileset/tiles.tres")
		add_child(node)
		tilemaps[layer] = node

	#for x in range(mapdic.width):
	#	for y in range(mapdic.height):
	#		var server_ids = get_real_server_ids(mapdic.map[x][y])
			
	#		set_cell(y + mapdic.starty,x + mapdic.startx, server_tile_id_to_local_id(server_ids[0]))
			
	#		if (server_ids[1] != -1):
	#			var overlay_id = server_tile_id_to_local_id(server_ids[1] * OVERLAY_MULT_FACTOR)
	#			var shape_id = server_ids[2]
				#TODO: Display overlay and shape :)
					
func server_tile_id_to_local_id(server_id):
	if server_tile_id_to_local_id_dic.has(server_id):
		return server_tile_id_to_local_id_dic[server_id]
	return TILE_UNKNOWN_TILE
					
func get_real_server_ids(packed_id):
    if packed_id & SHAPE_ID_MASK == 0:
        return [packed_id, -1]
    return [packed_id & BASE_ID_MASK, (packed_id & OVERLAY_ID_MASK) >> 5, (packed_id & SHAPE_ID_MASK) >> 10]				

func find_files(path, ends_with, exclude_dir):
	var files = []
	var dir = Directory.new()
	
	if dir.open(path) != OK:
		print("Cannot open " + path + "!")
		return
	
	dir.list_dir_begin(true)
	
	var file_name = dir.get_next()
	while (file_name != ""):
		if dir.current_is_dir():
			if file_name == exclude_dir:
				continue
				
			files += find_files(path+"/"+file_name, ends_with)
		elif file_name.ends_with(ends_with):
			files.push_back(path+"/"+file_name)
		file_name = dir.get_next()
	
	dir.list_dir_end()
	return files

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
		
		if (line.begins_with("L")):
			mapdic.layer = int(line.substr(3, line.length()-3))
			continue
			
		if (line.begins_with("X")):
			mapdic.startx = int(line.substr(3, line.length()-3))
			continue

		if (line.begins_with("Y")):
			mapdic.starty = int(line.substr(3, line.length()-3))
			continue

		if (line.begins_with("W")):
			mapdic.width = int(line.substr(3, line.length()-3))
			continue

		if (line.begins_with("H")):
			mapdic.height = int(line.substr(3, line.length()-3))
			break
		
	for x in range(mapdic.width):
		maparray.append([])
		maparray[x].resize(mapdic.height)
		
	while not (mapfile.eof_reached()):
		var line = mapfile.get_line()
		if (line == ""):
			break
		
		var values = line.split(";",false)
		maparray[int(values[0])][int(values[1])] = int(values[2])
	
	mapdic.map = maparray
	
	return mapdic