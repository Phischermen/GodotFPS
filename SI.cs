using Godot;
using System;

public static class SI
{
    //<summary>
    //Set the singleton, and just print an error if two singletons exist
    //</summary>
    public static void SetAndWarnUser<T>(ref T node, ref T singleton) where T: Node
    {
        if (singleton == null || node == null)
        {
            singleton = node;
        }
        else
        {
            GD.PushError("Two instances of " + node.GetType().Name + "instantiated: " + node.Name + " & " + singleton.Name);
        }
    }
}
