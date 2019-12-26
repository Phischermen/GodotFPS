using Godot;
using System;

public sealed class PlayerArms : Spatial
{
    private PlayerArms() { }
    private static PlayerArms _singleton;
    public static PlayerArms Singleton
    {
        get { return _singleton; }
        private set
        {
            SI.SetAndWarnUser(ref value, ref _singleton);
        }
    }

    public static readonly float SwingIncrement = 0.1f;
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
			_AnimationTree.Set("parameters/StateMachine/ArmBlend1D/blend_position", value);
		}
	}
	private Vector2 _swing;
	public Vector2 Swing
	{
		get{return _swing;}
		set
		{
			_swing = value;
			_AnimationTree.Set("parameters/StateMachine/ArmBlend1D/0/blend_position", value);
		}
	}
	private bool _retract;
	public bool Retract
	{
		get{return _retract;}
		set
		{
			_retract = value;
			_AnimationTree.Set("parameters/StateMachine/conditions/Retract", value);
			_AnimationTree.Set("parameters/StateMachine/conditions/Extend", !value);
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
		Jump = Land = Fall = Retract = false;
	}

    public override void _ExitTree()
    {
        Singleton = null;
    }
}
