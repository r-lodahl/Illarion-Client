extends Node2D

var http = preload("res://addons/rest/http_request.gd")

func _ready():
	var string = "{\"version\":\"8a4f22a8124ad61575a22ed9a86a1dedb60313fd\"}"
	
	var json = parse_json(string)
	
	printerr(string)
	printerr(json)
	printerr(typeof(json))
	
	
	
#	response.connect("loading", self, "_on_loading")
#	response.connect("loaded", self, "_on_loaded")
	
#func _on_loading(size, length):
#	print("size=",size,":length=",length)
	
#func _on_loaded(response):
#	print(response)