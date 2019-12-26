using Godot;
using System;

public sealed class PlayerAnimationTree : AnimationTree
{
    private PlayerAnimationTree() { }
    private static PlayerAnimationTree _singleton;
    public static PlayerAnimationTree Singleton
    {
        get { return _singleton; }
        private set
        {
            SI.SetAndWarnUser(ref value, ref _singleton);
        }
    }

    public AnimationNodeStateMachinePlayback StateMachineCrouch;
    private float _headBobBlend;
    public float HeadBobBlend
    {
        get { return _headBobBlend; }
        set
        {
            _headBobBlend = value;
            Set("parameters/HeadBob/blend_amount", value);
        }
    }

    private float _headBobScale;
    public float HeadBobScale
    {
        get { return _headBobScale; }
        set
        {
            _headBobScale = value;
            Set("parameters/TimeScaleHeadBob/scale", value);
        }
    }

    private float _landBlend;
    public float LandBlend
    {
        get { return _landBlend; }
        set
        {
            _landBlend = value;
            Set("parameters/Land/blend_amount", value);
        }
    }

    public override void _EnterTree()
    {
        base._EnterTree();
        Singleton = this;
        Active = true;
        StateMachineCrouch = (AnimationNodeStateMachinePlayback)Get("parameters/StateMachineCrouch/playback");
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        Singleton = null;
    }

    public void Land()
    {
        Set("parameters/OneShotLand/active", true);
    }

    public void Climb()
    {
        Set("parameters/OneShotClimb/active", true);
    }
}
