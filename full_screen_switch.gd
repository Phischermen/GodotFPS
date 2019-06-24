extends Node

func _input(event: InputEvent) -> void:
	if Input.is_action_just_pressed("fullscreen"):
		OS.window_fullscreen = !OS.window_fullscreen;
		get_tree().set_input_as_handled();
