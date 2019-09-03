extends ViewportContainer

var on:bool;
var viewport:Viewport;
var camera:Camera;
var playerCamera:Camera;
export(NodePath) var playerCameraPath;

func _ready():
	viewport = get_node("Viewport");
	camera = get_node("Viewport/Camera");
	playerCamera = get_node(playerCameraPath);
	
func _process(delta):
	if Input.is_action_just_pressed("place_debug_camera"):
		on = !on;
		visible = on;
		camera.global_transform = playerCamera.global_transform;