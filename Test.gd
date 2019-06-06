extends Node2D

export(TileSet) var tileset

#func _process(delta):
#	update()

func _draw():
	var id = tileset.find_tile_by_name("grass-0")
	#draw_texture(tileset.tile_get_texture(id), Vector2(3,3))
	draw_texture_rect_region(tileset.tile_get_texture(id), Rect2(10,10,76,37), tileset.tile_get_region(id))