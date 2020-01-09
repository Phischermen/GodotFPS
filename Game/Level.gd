extends Node

var game_scene:PackedScene = preload("res://Game/Game.tscn");

#Room.gd sets both these to true when user tests room
export var test_mode:bool = false;
export var spawn_flying:bool = false; 

func _ready() -> void:
	if get_tree().root.get_node_or_null("/root/" + name) == self:
		var game = game_scene.instance(PackedScene.GEN_EDIT_STATE_DISABLED);
		get_tree().root.call_deferred("add_child", game);
		get_parent().call_deferred("remove_child", self);
		game.get_node("CurrentLevel").call_deferred("add_child", self);
		set_deferred("owner", game.get_node("CurrentLevel"));
		game.current_level = self;
	$SpawnPoint.visible = false;
	#$LookAt.visible = false;
	pass

