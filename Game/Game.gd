extends Node

var player_scene:PackedScene = preload("res://Character/Player.tscn");

var player;
var current_level;

var save_data;

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	player = player_scene.instance(PackedScene.GEN_EDIT_STATE_DISABLED);
	spawn_player();
	save_data = player.call("Save");
	pass

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	if Input.is_action_just_pressed("quick_save"):
		save_data = player.call("Save");
	if Input.is_action_just_pressed("quick_load"):
		player.call("Load", save_data);
	pass

func spawn_player():
	# Player is already instantiated, just move it to spawn point
	current_level.get_node("Scene").add_child(player);
	player.translation = current_level.get_node("SpawnPoint").translation;
	player.call("LookAt", current_level.get_node("LookAt").translation);
	if (current_level.spawn_flying):
		player.set("Flying", true);