extends Node2D

const title = "Illarion Client v2.0"

func _process(delta):
	OS.set_window_title(title + " | FPS: " + str(Engine.get_frames_per_second()))

###########################
# DYNAMICALLY MAP LOADING #
###########################
var map
var server_tile_id_to_local_id_dic = {}
var tilemaps = {}

const MAP_LOADER = preload("map_loader.gd")
const TABLE_LOADER = preload("table_loader.gd")

const TILE_UNKNOWN_TILE = 306
const ID_COLUMN_TILES = 9 #10
const ID_COLUMN_OVERLAYS = 2 #3
const NAME_COLUMN_TILES = 3 #4
const NAME_COLUMN_OVERLAYS = 3 #4

func _ready():
	var tileset = load("res://assets/tileset/tiles.res")
	TABLE_LOADER.create_mapping_table(tileset, "res://assets/tileset/tiles.tbl", NAME_COLUMN_TILES, ID_COLUMN_TILES, TILE_UNKNOWN_TILE, server_tile_id_to_local_id_dic)
	TABLE_LOADER.create_mapping_table(tileset, "res://assets/tileset/overlays.tbl", NAME_COLUMN_OVERLAYS, ID_COLUMN_OVERLAYS, TILE_UNKNOWN_TILE, server_tile_id_to_local_id_dic)
	map = MAP_LOADER.load_map()
	create_tilemaps()

func create_tilemaps():
	
	print("Starting creation of world")
	
	# Initial map display
	map.setup_tile_data(get_child(0), get_child(1), server_tile_id_to_local_id_dic)
	map.set_mapcenter(0,0,0)
	
	# Distribute events
	get_child(2).connect("moved_one_tile",map, "_mapcenter_was_moved")
	
	print("Finished")
				
