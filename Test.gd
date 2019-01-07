extends Node2D

var http = preload("res://addons/rest/http_request.gd")

func _ready():
	var request = http.new()
	
	var response = request.sget("https://api.github.com", "/repos/Illarion-ev/Illarion-Map/commits/master", 443, true, [])
	
	response.connect("loading", self, "_on_loading")
	response.connect("loaded", self, "_on_loaded")
	
func _on_loading(size, length):
	print("size=",size,":length=",length)
	
func _on_loaded(response):
	print(response)