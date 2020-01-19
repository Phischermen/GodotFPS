using Godot;
using System;

public abstract class Interactable : Node
{
    [Export]
    private NodePath HighlightPath;
    private MeshInstance HighlightMesh;

    public bool Highlight 
    {
        get { return HighlightMesh.Visible; }
        set
        {
            HighlightMesh.Visible = value;
        }
    }

    public override void _Ready()
    {
        base._Ready();
        HighlightMesh = GetNode<MeshInstance>(HighlightPath);
    }

    //When player interacts with this node, do something
    public abstract void Interact();
}
