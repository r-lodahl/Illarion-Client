extends Node2D

func _process(delta):
	OS.set_window_title(str(Engine.get_frames_per_second()))
