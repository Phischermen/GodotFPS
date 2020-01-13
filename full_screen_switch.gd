extends Node

func _ready() -> void:
	pause_mode = Node.PAUSE_MODE_PROCESS;
	
func _input(event: InputEvent) -> void:
	if Input.is_action_just_pressed("fullscreen"):
		OS.window_fullscreen = !OS.window_fullscreen;
		get_tree().set_input_as_handled();
