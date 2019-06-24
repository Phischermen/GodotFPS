tool
extends RayCast

export(float) var angle:float setget set_angle
export(float) var radius:float setget set_radius

func set_angle(value):
	angle = value;
	var rad = deg2rad(angle);
	translation = Vector3(sin(rad)*radius,1,cos(rad)*radius);

func set_radius(value):
	radius = value;
	set_angle(angle);
# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.

# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta):
#	pass
