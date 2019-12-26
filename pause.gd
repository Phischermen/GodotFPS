extends Node

onready var player = preload("res://Character/Scripts/Player.cs").get_Singleton();
onready var save_data = player.call("Save");
	
func _process(delta):
	if Input.is_action_just_pressed("ui_cancel"):
		match(Input.get_mouse_mode()):
			Input.MOUSE_MODE_CAPTURED:
				Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE);
			Input.MOUSE_MODE_VISIBLE:
				Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED);
	if Input.is_action_just_pressed("restart"):
		get_tree().reload_current_scene();
	if Input.is_action_just_pressed("quick_save"):
		save_data = player.call("Save");
	if Input.is_action_just_pressed("quick_load"):
		player.call("Load", save_data);
