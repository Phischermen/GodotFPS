extends WindowDialog

var on:bool = false;
var viewport:Viewport;
var camera:Camera;
var player:Spatial;

func _ready():
	viewport = get_node("ViewportContainer/Viewport");
	camera = viewport.get_node("Camera");
	player = preload("res://Character/Scripts/Player.cs").get_Singleton();
	
func _process(delta):
	if Input.is_action_just_pressed("place_debug_camera"):
		on = !on;
		popup();
		camera.global_transform = player.global_transform;