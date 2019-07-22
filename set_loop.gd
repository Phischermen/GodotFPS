extends AnimationPlayer

"""
Since there appears to be a bug with setting loop and autoplay properties in editor for an imported .dae scene, they will need to be set via script.
"""
export(String) var auto:String;
export(Array,String) var loops = [""];

func _ready():
	autoplay = auto;
	for loop in loops:
		var anim:Animation = get_animation(loop);
		if anim != null:
			anim.loop = true;
	pass
	
func _process(delta):
	if Input.is_action_just_pressed("fire1"):
		play("ArmatureAction");
