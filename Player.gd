extends KinematicBody2D

var current_position = Vector2(0,0)

func _ready():
	current_position = position
	set_process_input(true)
	set_process(true)

func _input(event):
	if Input.is_action_pressed("move_up"):
		current_position.y -= 100
	elif Input.is_action_pressed("move_down"):
		current_position.y += 100
	elif Input.is_action_pressed("move_left"):
		current_position.x -= 100
	elif Input.is_action_pressed("move_right"):
		current_position.x += 100

func _process(delta):
	position = current_position