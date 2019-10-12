extends WindowDialog

var on:bool = false;
var viewport:Viewport;
var camera:Camera;
var playerCamera:Camera;
export(NodePath) var playerCameraPath;
export(NodePath) var debugCameraPath;
export(NodePath) var ViewportPath;

func _ready():
	viewport = get_node(ViewportPath);
	camera = get_node(debugCameraPath);
	playerCamera = get_node(playerCameraPath);
	
func _process(delta):
	if Input.is_action_just_pressed("place_debug_camera"):
		on = !on;
		popup();
		camera.global_transform = playerCamera.global_transform;
