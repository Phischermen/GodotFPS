tool
extends RayCast

onready var shadow:MeshInstance;

func _ready():
	shadow = $BlobShadow;
	print(shadow);
	pass

func _process(delta: float) -> void:
	transform.basis = get_parent().transform.basis.inverse();
	if is_colliding():
		visible = true;
		shadow.translation = to_local(get_collision_point());
		shadow.scale = Vector3.ONE * (1 - shadow.translation.y / -10);
		shadow.translation.y += 0.1;
		#TODO figure out how to make shadow face up
		var from = shadow.transform.basis.z
		var to = get_collision_normal();
		var axis:Vector3 = from.cross(to).normalized();
		
		var angle:float = from.angle_to(to);
		if abs(angle) > 0.01:
			print(angle);
			var rotated_basis = shadow.transform.basis.rotated(axis, angle);
			shadow.transform.basis = rotated_basis;
			
	else:
		visible = false;
	pass