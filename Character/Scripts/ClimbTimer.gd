extends Timer
"""
Since the player may press the climb button too early, start a timer when the climb action is just pressed to indicate for the next couple moments the player wants to climb.
"""
export(float) var input_overtime:float = 0.5;
var wants_to_climb:bool = false;

func _ready():
	connect("timeout",self,"set",["wants_to_climb",false]);
	pass;

func _input(event):
	if Input.is_action_just_pressed("climb"):
		wants_to_climb = true;
		start(input_overtime);
		get_tree().set_input_as_handled();
	pass;