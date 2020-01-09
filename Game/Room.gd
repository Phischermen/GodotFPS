extends Spatial

export(PackedScene) var level_scene:PackedScene;

func _ready() -> void:
	if get_tree().root.get_node_or_null("/root/" + name) == self:
		var level = level_scene.instance(PackedScene.GEN_EDIT_STATE_DISABLED);
		get_tree().root.call_deferred("add_child", level);
		get_parent().call_deferred("remove_child", self);
		level.get_node("Scene").call_deferred("add_child", self);
		set_deferred("owner", level.get_node("Scene"));
		level.test_mode = true;
		level.spawn_flying = true;
		#Level.gd will now continue with debugging process
		
	pass # Replace with function body.
