using Godot;
using System;

public class LiftSystem : Node
{
    private LiftSystem() { }
    private static LiftSystem _singleton;
    public static LiftSystem Singleton
    {
        get { return _singleton; }
        private set
        {
            SI.SetAndWarnUser(ref value, ref _singleton);
        }
    }

    private MeshInstance PlacementPreview;
    private static ILift CarriedNode;
    public static bool Carrying;

    public override void _EnterTree()
    {
        Singleton = this;
    }

    public override void _ExitTree()
    {
        Singleton = null;
    }

    public override void _Ready()
    {
        PlacementPreview = GetNode<MeshInstance>("PlacementPreview");
    }

    public static void SetPreview(Mesh mesh, Material material)
    {
        Singleton.PlacementPreview.Mesh = mesh;
        Singleton.PlacementPreview.SetSurfaceMaterial(0, material);
    }

    public static void OrientPreview(Vector3 target, Vector3 up)
    {
        Singleton.PlacementPreview.GlobalTransform = Singleton.PlacementPreview.GlobalTransform.LookingAt(target, up);
    }

    public static void LerpOrientPreview(Vector3 target, Vector3 up)
    {
        Singleton.PlacementPreview.GlobalTransform = Singleton.PlacementPreview.GlobalTransform.InterpolateWith(Singleton.PlacementPreview.GlobalTransform.LookingAt(target, up), 0.1f);
    }

    public static void PositionPreview(Vector3 position)
    {
        Singleton.PlacementPreview.Translation = position;
    }

    public static void LerpPositionPreview(Vector3 position)
    {
        Singleton.PlacementPreview.Translation = Singleton.PlacementPreview.Translation.LinearInterpolate(position, 0.1f);
    }
    
    public static void Lift(ILift node)
    {
        if (node.CanLift() && !Carrying)
        {
            Carrying = true;
            CarriedNode = node;

            PlayerReticle.Singleton.Visible = false;

            node.ShowAndActivate(false);
            Singleton.PlacementPreview.Visible = true;
            SetPreview(node.GetPreviewMesh(), node.GetPreviewMaterial());
            
        }
    }

    public static void Drop()
    {
        if (Carrying)
        {
            Carrying = false;

            PlayerReticle.Singleton.Visible = true;

            //Move CarriedNode to preview
            CarriedNode.GetOwner().GlobalTransform = Singleton.PlacementPreview.GlobalTransform;

            //Show CarriedNode
            CarriedNode.ShowAndActivate(true);
            Singleton.PlacementPreview.Visible = false;
        }
        
    }
}
