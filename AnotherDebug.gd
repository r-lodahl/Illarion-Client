extends KinematicBody2D

func _process(delta):
	if (Input.is_action_just_pressed("move_down")):
		translate(Vector2(0,10))
	