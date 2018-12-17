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
onready var tileset = load("res://assets/tileset/tiles.res")

const MAP_LOADER = preload("map_loader.gd")
const TABLE_LOADER = preload("table_loader.gd")

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
	TABLE_LOADER.create_mapping_table(tileset, "res://assets/tileset/tiles.tbl", NAME_COLUMN_TILES, ID_COLUMN_TILES, TILE_UNKNOWN_TILE, server_tile_id_to_local_id_dic)
	TABLE_LOADER.create_mapping_table(tileset, "res://assets/tileset/overlays.tbl", NAME_COLUMN_OVERLAYS, ID_COLUMN_OVERLAYS, TILE_UNKNOWN_TILE, server_tile_id_to_local_id_dic)
	map = MAP_LOADER.load_map()
	create_tilemaps()

func create_tilemaps():
	
	print("Starting creation of world")
	
	var node = TileMap.new()
	node.cell_size = Vector2(76,37)
	node.mode = TileMap.MODE_ISOMETRIC
	node.tile_set = tileset
	add_child(node)
	
	var node2 = TileMap.new()
	node2.cell_size = Vector2(76,37)
	node2.mode = TileMap.MODE_ISOMETRIC
	node2.tile_set = tileset
	add_child(node2)

	map.setup_tile_data(node, node2, tileset, server_tile_id_to_local_id_dic)
	map.reload_visible_tilemap(0,0,0)

	print("Finished")
				
