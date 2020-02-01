using Godot;
using System;

public class CrateHealthManager : HealthManager
{
    [Export]
    public PackedScene FracturedCrate;

    private CrateHealthManager() : base(100, 100) { }

    public override void Kill(string deathSource)
    {
        base.Kill(deathSource);

        //Spawn fractured version outside of owner
        Spatial node = (Spatial)FracturedCrate.Instance();
        Owner.GetParent().AddChild(node);
        node.Owner = Owner.GetParent();
        node.GlobalTransform = ((Spatial)Owner).GlobalTransform;

        //Free myself
        Owner.QueueFree();
    }
}
