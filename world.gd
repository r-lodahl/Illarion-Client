extends Node2D

const title = "Illarion Client v2.0"

func _process(delta):
	OS.set_window_title(title + " | FPS: " + str(Engine.get_frames_per_second()))

###########################
# DYNAMICALLY MAP LOADING #
###########################
var map
#const VISUAL_MAP = preload("res://VisualMap.gd")
const FILE_OP = preload("res://file_operations.gd")
const TABLE_LOADER = preload("res://table_loader.gd")

func _ready():
	print("Starting creation of world")
	
	# Build item dictionary
	var local_items = FILE_OP.find_files("res://assets/spritesets/items.sprites", ".tres")
	var item_dic = TABLE_LOADER.create_item_table("res://assets/spritesets/items.tbl", local_items)
	
	
	
	
	# Initial map display
	#map = VISUAL_MAP.new()
	#map.setup(get_child(0), get_child(1), item_dic)
	#map.reload_map_at(0,0,0)
	
	# Distribute events
	#get_child(2).connect("moved_one_tile", map, "_mapcenter_was_moved")
	
	print("Finished")