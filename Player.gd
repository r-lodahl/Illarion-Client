extends KinematicBody2D

var current_position = Vector2(0,0)

signal moved_one_tile(x,y)

func _ready():
	current_position = position
	set_process_input(true)
	set_process(true)

func _input(event):
	if Input.is_action_pressed("move_up"):
		current_position.y -= 18.5
		current_position.x -= 38
		emit_signal("moved_one_tile",0,-1)
	elif Input.is_action_pressed("move_down"):
		current_position.y += 18.5
		current_position.x += 38
		emit_signal("moved_one_tile",0,1)
	elif Input.is_action_pressed("move_left"):
		current_position.x -= 38
		current_position.y += 18.5
		emit_signal("moved_one_tile",-1,0)
	elif Input.is_action_pressed("move_right"):
		current_position.x += 38
		current_position.y -= 18.5
		emit_signal("moved_one_tile",1,0)

func _process(delta):
	position = current_position