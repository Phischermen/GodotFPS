using Godot;
using System;

public class SnapArea : Area
{
    public bool Snap;
    private void OnSnapAreaBodyEntered(object body)
    {
        Snap = true;
        //GD.Print(Snap);
    }

}


