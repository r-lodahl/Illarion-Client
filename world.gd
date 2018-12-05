extends Node2D

const title = "Illarion Client v2.0"

func _process(delta):
	OS.set_window_title(title + " | FPS: " + str(Engine.get_frames_per_second()))

###########################
# DYNAMICALLY MAP LOADING #
###########################
var server_tile_id_to_local_id_dic = {}
var maps = {}
var tilemaps = {}
var tileset

const MAP_LOADER = require("map_loader.gd")
const TABLE_LOADER = require("table_loader.gd")

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
	tileset = load("res://assets/tileset/tiles.tres")
	server_tile_id_to_local_id_dic += TABLE_LOADER.create_mapping_table("res://assets/tileset/tiles.tbl", NAME_COLUMN_TILES, ID_COLUMN_TILES, TILE_UNKNOWN_TILE)
	server_tile_id_to_local_id_dic += TABLE_LOADER.create_mapping_table("res://assets/tileset/overlays.tbl", NAME_COLUMN_OVERLAYS, ID_COLUMN_OVERLAYS, TILE_UNKNOWN_TILE)
	maps = MAP_LOADER.load_maps()
	create_tilemaps()

func create_tilemaps():
	# Create the maps
	for layer in maps.keys.sort():
		var node = TileMap.new()
		node.cell_size = Vector3(76,37,0)
		node.cell_y_sort = true
		node.mode = MODE_ISOMETRIC
		node.tile_set = tileset
		add_child(node)
		tilemaps[layer] = node

	# For every tile (x/y) on every map on every layer: put it on its tilemap.
	for layermaps in maps:
		for map in layermaps:
			for x in range(map.width):
				for y in range(map.height):
					var server_ids = get_real_server_ids(map.map[x][y])
					tilemaps[map.layer].set_cell(x+mapdic.startx,y+mapdic.starty,server_tile_id_to_local_id(server_ids[0]))
	
	
	#for x in range(mapdic.width):
	#	for y in range(mapdic.height):
	#		var server_ids = get_real_server_ids(mapdic.map[x][y])
			
	#		set_cell(y + mapdic.starty,x + mapdic.startx, server_tile_id_to_local_id(server_ids[0]))
			
	#		if (server_ids[1] != -1):
	#			var overlay_id = server_tile_id_to_local_id(server_ids[1] * OVERLAY_MULT_FACTOR)
	#			var shape_id = server_ids[2]
				#TODO: Display overlay and shape :)
					
static func server_tile_id_to_local_id(server_id):
	if server_tile_id_to_local_id_dic.has(server_id):
		return server_tile_id_to_local_id_dic[server_id]
	return TILE_UNKNOWN_TILE
						
static func get_real_server_ids(packed_id):
	if packed_id & SHAPE_ID_MASK == 0:
		return [packed_id, -1]
	return [packed_id & BASE_ID_MASK, (packed_id & OVERLAY_ID_MASK) >> 5, (packed_id & SHAPE_ID_MASK) >> 10]