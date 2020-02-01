using Godot;

public interface ILift
{
    RigidBody GetOwner();
    Mesh GetLiftMesh();
    Material GetLiftMaterial();
    Mesh GetPreviewMesh();
    Material GetPreviewMaterial();
    void ShowAndActivate(bool activate);
    bool CanLift();
}


