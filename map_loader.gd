const FILE_OP = preload("file_operations.gd")
const VISUAL_MAP = preload("res://VisualMap.gd")

static func load_map():
	var files = FILE_OP.find_files("res://assets/map", ".tiles.txt", "Testmaps")
	
	var maps = {}
	for file in files:
		var mapdic = load_single_map(file)
		
		if not maps.has(mapdic.layer): maps[mapdic.layer] = []
		maps[mapdic.layer].push_back(mapdic)
	
	var layers = maps.keys().duplicate()
	layers.sort()
	
	var visual_map = VISUAL_MAP.new()
	visual_map.setup_map_data(maps, layers)
	
	return visual_map
	
# This function absolutly relies on the correct map scheme
# This means the sequence is comments LXYWH tiles beginning with 0 0, 0 1, etc.
static func load_single_map(filepath):
	print("Loading: " + filepath)
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