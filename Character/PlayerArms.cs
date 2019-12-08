using Godot;
using System;

public class PlayerArms : Spatial
{
    public static PlayerArms Singleton;
	public AnimationTree _AnimationTree;
	
	private bool _jump;
	public bool Jump
	{
		get{return _jump;}
		set
		{
			_jump = value;
			_AnimationTree.Set("parameters/StateMachine/conditions/Jump", value);
		}
	}
	private bool _land;
	public bool Land
	{
		get{return _land;}
		set
		{
			_land = value;
			_AnimationTree.Set("parameters/StateMachine/conditions/Land", value);
		}
	}
	private bool _fall;
	public bool Fall
	{
		get{return _fall;}
		set
		{
			_fall = value;
			_AnimationTree.Set("parameters/StateMachine/conditions/Fall", value);
		}
	}
	private float _walk;
	public float Walk
	{
		get{return _walk;}
		set
		{
			_walk = value;
			_AnimationTree.Set("parameters/StateMachine/WalkBlend/blend_position", value);
		}
	}

    public override void _EnterTree()
    {
        if (Singleton == null)
        {
            Singleton = this;
        }
        else
        {
            GD.Print("Two instances of PlayerArm class instantiated: ", Name, " & ", Singleton.Name);
        }
    }
	
	public override void _Ready()
	{
		_AnimationTree = GetNode<AnimationTree>("AnimationTree");
		_AnimationTree.Active = true;
		Walk = 0f;
		Jump = Land = Fall = false;
	}

    public override void _ExitTree()
    {
        Singleton = null;
    }
}
