extends CanvasLayer

func _ready() -> void:
	#Allow shaders to compile
	yield(get_tree(),"idle_frame")
	for child in get_children():
		child.visible = false;
