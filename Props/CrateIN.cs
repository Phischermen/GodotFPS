using Godot;
using Godot.Collections;

public class CrateIN : Interactable, ILift
{
    [Export]
    private NodePath LiftMeshPath;
    private MeshInstance LiftMesh;
    [Export]
    private NodePath PreviewMeshPath;
    private MeshInstance PreviewMesh;
    private Array<CollisionShape> CollisionShapes;

    RigidBody ILift.GetOwner()
    {
        return (RigidBody)Owner;
    }

    bool ILift.CanLift()
    {
        return true;
    }

    void ILift.ShowAndActivate(bool activate)
    {
        RigidBody owner = (RigidBody)Owner;
        owner.Visible = activate;
        foreach(CollisionShape shape in CollisionShapes)
        {
            shape.Disabled = !activate;
        }
        owner.SetAngularVelocity(Vector3.Zero);
        owner.SetLinearVelocity(Vector3.Zero);
    }

    Mesh ILift.GetLiftMesh()
    {
        return LiftMesh.Mesh;
    }

    Material ILift.GetLiftMaterial()
    {
        return LiftMesh.GetSurfaceMaterial(0);
    }

    Mesh ILift.GetPreviewMesh()
    {
        return PreviewMesh.Mesh;
    }

    Material ILift.GetPreviewMaterial()
    {
        return PreviewMesh.GetSurfaceMaterial(0);
    }

    public override void _Ready()
    {
        base._Ready();
        LiftMesh = GetNode<MeshInstance>(LiftMeshPath);
        PreviewMesh = GetNode<MeshInstance>(PreviewMeshPath);
        CollisionShapes = NX.FindAll<CollisionShape>(Owner, true);
    }

    public override void Interact()
    {
        LiftSystem.Lift(this);
    }

    
}
