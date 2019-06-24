extends Node

func _process(delta):
	if Input.is_action_just_pressed("ui_cancel"):
		match(Input.get_mouse_mode()):
			Input.MOUSE_MODE_CAPTURED:
				Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE);
			Input.MOUSE_MODE_VISIBLE:
				Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED);
	if Input.is_action_just_pressed("restart"):
		get_tree().reload_current_scene();