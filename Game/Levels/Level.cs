using Godot;
using System;

public class Level : Node
{
    private static readonly string GameScenePath = "res://Game/Game.tscn";

    private static readonly string[] HideNodes = {
        "SpawnPoint",
        "LookAt"
    };

    PackedScene GameScene;

    //Room.cs sets both these to true when user tests room
    [Export]
    public bool TestMode = false;
    [Export]
    public bool SpawnFlying = false;

    public override void _Ready()
    {
        if (NX.IsUnderRoot(this))
        {
            //Load Game Scene
            GameScene = ResourceLoader.Load<PackedScene>(GameScenePath);

            //Instance game scene
            Node Game = GameScene.Instance();

            //Add to tree
            GetTree().Root.CallDeferred("add_child", Game);

            //Remove us from tree
            GetParent().CallDeferred("remove_child", this);

            //Add under Game node's 'CurrentLevel' and set owner
            Game.GetNode("CurrentLevel").CallDeferred("add_child", this);
            SetDeferred("owner", Game.GetNode("CurrentLevel"));

            Game.Set("current_level", this);
        }
        foreach(string path in HideNodes)
        {
            GetNode<Spatial>(path).Visible = false;
        }
    }
}
