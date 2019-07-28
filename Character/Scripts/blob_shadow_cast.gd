extends RayCast

onready var shadow:MeshInstance;

export(float) var shadow_scale = 1.0;

func _ready():
	shadow = $BlobShadow;
	
	pass

func update(offset:float) -> void:
	#ensure raycast is always facing downwards
	transform.basis = get_parent().transform.basis.inverse();
	if is_colliding():
		shadow.visible = true;
		
		#put shadow slightly above ground to avoid z-fighting
		shadow.translation = to_local(get_collision_point());
		shadow.translation.y += offset;
		
		#scale shadow based on distance to floor
		shadow.scale = Vector3.ONE * shadow_scale * (1 - shadow.translation.y / -cast_to.length());
		
		#get direction shadow is facing
		var from:Vector3 = shadow.transform.basis.z;
		
		#get normal of ground
		var to:Vector3 = get_collision_normal();
		
		#get the axis defined by these two vectors
		var axis:Vector3 = from.cross(to).normalized();
		
		#get angle from -> to
		var angle:float = from.angle_to(to);
		
		if abs(angle) > 0.01:
			#angle is significant
			var rotated_basis:Basis = shadow.transform.basis.rotated(axis, angle);
			shadow.transform.basis = rotated_basis;
			
	else:
		shadow.visible = false;
	pass