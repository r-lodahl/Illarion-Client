extends Node2D

func _ready():
	var sprite = Sprite.new()
	sprite.texture = load("res://assets/spritesets/items.sprites/zwergenaxt.tres")
	sprite.global_position = Vector2(40.0,40.0)
	add_child(sprite)