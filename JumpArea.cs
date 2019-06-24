using Godot;
using System;

public class JumpArea : Area
{
    public int Bodies = 0;
    //public bool Snap = true;
    public bool CanBunnyHop = true;

    private void OnJumpAreaBodyEntered(object body)
    {
        //Increment number of bodies
        Bodies += 1;

        //Snap to floor
        //Snap = true;

        //Allow character to bunny hop again
        CanBunnyHop = true;
        //GD.Print(CanBunnyHop);
    }

    private void OnJumpAreaBodyExited(object body)
    {
        //Decrement number of bodies
        Bodies -= 1;
        //GD.Print(Bodies);
    }
}



