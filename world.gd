extends Node2D

const title = "Illarion Client v2.0"

func _process(delta):
	OS.set_window_title(title + " | FPS: " + str(Engine.get_frames_per_second()))

###########################
# DYNAMICALLY MAP LOADING #
###########################
var map
const VISUAL_MAP = preload("res://VisualMap.gd")

func _ready():
	print("Starting creation of world")
	
	# Initial map display
	map = VISUAL_MAP.new()
	map.setup_tile_data(get_child(0), get_child(1))
	map.set_mapcenter(0,0,0)
	
	# Distribute events
	get_child(2).connect("moved_one_tile",map, "_mapcenter_was_moved")
	
	print("Finished")
				
