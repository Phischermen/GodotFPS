extends SpotLight

func _input(event):
	if event is InputEventKey:
		if Input.is_action_just_pressed("flashlight"):
			visible = !visible;
			get_tree().set_input_as_handled();
	pass;
