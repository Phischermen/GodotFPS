using Godot;
using System;

public class CrateIN : Interactable
{
    public override void Interact()
    {
        GD.Print("Pickup " + Name);
    }
}
