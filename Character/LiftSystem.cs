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
    public static bool Carrying { get; private set; }

    public static Vector3 Velocity { get; private set; }
    public static Vector3 Acceleration { get; private set; }
    public static Vector3 AngularAcceleration { get; private set; }

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

        //Reset Acceleration
        AngularAcceleration = Vector3.Zero;
    }

    public static void LerpOrientPreview(Vector3 target, Vector3 up, float speed, float time)
    {
        Vector3 o = Singleton.PlacementPreview.Rotation;
        Singleton.PlacementPreview.GlobalTransform = Singleton.PlacementPreview.GlobalTransform.InterpolateWith(Singleton.PlacementPreview.GlobalTransform.LookingAt(target, up), speed * time);
        Vector3 d = Singleton.PlacementPreview.Rotation;
        //Calculate AngularAcceleration
        AngularAcceleration = 2f * (d - o) / Mathf.Pow(time, 2f);
    }

    public static void PositionPreview(Vector3 position)
    {
        Singleton.PlacementPreview.Translation = position;

        //Reset Acceleration & Velocity
        Velocity = Vector3.Zero;
        Acceleration = Vector3.Zero;
    }

    public static void LerpPositionPreview(Vector3 position, float speed, float time)
    {
        Vector3 o = Singleton.PlacementPreview.Translation;
        Vector3 d = Singleton.PlacementPreview.Translation.LinearInterpolate(position, speed * time);
        Singleton.PlacementPreview.Translation = d;

        //Calculate Acceleration
        Vector3 distance = (d - o);
        Velocity = distance / time;
        Acceleration = 2f * distance / Mathf.Pow(time, 2f);

        //GD.Print(Velocity);
        //GD.Print(Acceleration);
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
            PositionPreview(node.GetOwner().GlobalTransform.origin);
        }
    }

    public static void Drop(float force = 1f)
    {
        if (Carrying)
        {
            Carrying = false;

            PlayerReticle.Singleton.Visible = true;

            //Move CarriedNode to preview
            CarriedNode.GetOwner().GlobalTransform = Singleton.PlacementPreview.GlobalTransform;

            //Apply force to CarriedNode
            float mass = CarriedNode.GetOwner().Mass;
            CarriedNode.GetOwner().LinearVelocity = Velocity + Player.Singleton.MovementVelocity;
            CarriedNode.GetOwner().AddCentralForce(mass * force * Acceleration);
            CarriedNode.GetOwner().AddCentralForce(force * Player.Singleton.MovementForce);
            CarriedNode.GetOwner().AddTorque(mass * force * AngularAcceleration);

            //Show CarriedNode
            CarriedNode.ShowAndActivate(true);
            Singleton.PlacementPreview.Visible = false;
        }
        
    }
}
