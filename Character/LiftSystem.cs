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

    public KinematicBody PlacementPreview { get; private set; }
    private Area PlacementArea;
    private MeshInstance PreviewMesh;
    private BoxShape PreviewBoxShape;
    public static ILift CarriedNode { get; private set; }
    private static bool AreaClear = true;
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

    public void _OnAreaBodyEntered(object body)
    {
        AreaClear = false;
        //GD.Print("Entered");
    }

    public void _OnAreaBodyExited(object body)
    {
        AreaClear = PlacementArea.GetOverlappingBodies().Count == 1;
        //GD.Print("Exited");
    }

    public override void _Ready()
    {
        PlacementPreview = GetNode<KinematicBody>("PlacementPreview");
        PreviewMesh = GetNode<MeshInstance>("PreviewMesh");
        PlacementArea = NX.GetNodeWithNameType<Area>(PreviewMesh);
        PlacementArea.Connect("body_entered", this, "_OnAreaBodyEntered");
        PlacementArea.Connect("body_exited", this, "_OnAreaBodyExited");
        PreviewBoxShape = (BoxShape)NX.GetNodeWithNameType<CollisionShape>(PlacementPreview).Shape;
    }

    public static void SetPreview(Mesh mesh, Material material)
    {
        Singleton.PreviewMesh.Mesh = mesh;
        Singleton.PreviewBoxShape.Extents = mesh.GetAabb().Size / 2f;
        Singleton.PreviewMesh.SetSurfaceMaterial(0, material);
    }

    public static Vector3 CastBoxFromPlayer(float distance)
    {
        //Goto Player
        Singleton.PlacementPreview.GlobalTransform = Player.Singleton.ScanRay.GlobalTransform;

        Vector3 rel = -(Singleton.PlacementPreview.GlobalTransform.basis.z * distance);
        KinematicCollision collision = Singleton.PlacementPreview.MoveAndCollide(rel, testOnly: true);
        return (collision == null) ? Singleton.PlacementPreview.Translation + rel : Singleton.PlacementPreview.Translation + collision.Travel;
    }

    public static void OrientPreview(Vector3 target, Vector3 up)
    {
        Singleton.PreviewMesh.GlobalTransform = Singleton.PreviewMesh.GlobalTransform.LookingAt(target, up);

        //Reset Acceleration
        AngularAcceleration = Vector3.Zero;
    }

    public static void LerpOrientPreview(Vector3 target, Vector3 up, float speed, float time)
    {
        Vector3 o = Singleton.PreviewMesh.Rotation;
        Singleton.PreviewMesh.GlobalTransform = Singleton.PreviewMesh.GlobalTransform.InterpolateWith(Singleton.PreviewMesh.GlobalTransform.LookingAt(target, up), speed * time);
        Vector3 d = Singleton.PreviewMesh.Rotation;

        //Calculate AngularAcceleration
        AngularAcceleration = 2f * (d - o) / Mathf.Pow(time, 2f);
    }

    public static void PositionPreview(Vector3 position)
    {
        Singleton.PreviewMesh.Translation = position;

        //Reset Acceleration & Velocity
        Velocity = Vector3.Zero;
        Acceleration = Vector3.Zero;
    }

    public static void LerpPositionPreview(Vector3 position, float speed, float time)
    {
        Vector3 o = Singleton.PreviewMesh.Translation;
        Vector3 d = Singleton.PreviewMesh.Translation.LinearInterpolate(position, speed * time);
        Vector3 distance = d - o;
        Singleton.PreviewMesh.Translation = d;

        //Calculate Acceleration
        Velocity = distance / time;
        Acceleration = 2f * distance / Mathf.Pow(time, 2f);
    }
    
    public static void Lift(ILift node)
    {
        if (node.CanLift() && !Carrying)
        {
            Carrying = true;
            CarriedNode = node;

            PlayerReticle.Singleton.Visible = false;

            node.ShowAndActivate(false);

            Singleton.PreviewMesh.Visible = true;
            SetPreview(node.GetPreviewMesh(), node.GetPreviewMaterial());
            PositionPreview(node.GetOwner().GlobalTransform.origin);
        }
    }

    public static void Drop(float force = 1f)
    {
        if (Carrying && AreaClear)
        {
            Carrying = false;

            PlayerReticle.Singleton.Visible = true;

            //Move CarriedNode to preview
            CarriedNode.GetOwner().GlobalTransform = Singleton.PreviewMesh.GlobalTransform;

            //Apply force to CarriedNode
            float mass = CarriedNode.GetOwner().Mass;
            CarriedNode.GetOwner().LinearVelocity = Velocity + Player.Singleton.MovementVelocity;
            CarriedNode.GetOwner().AddCentralForce(mass * force * Acceleration);
            CarriedNode.GetOwner().AddCentralForce(force * Player.Singleton.MovementForce);
            CarriedNode.GetOwner().AddTorque(mass * force * AngularAcceleration);

            //Show CarriedNode
            CarriedNode.ShowAndActivate(true);
            Singleton.PreviewMesh.Visible = false;
        }
        
    }
}
