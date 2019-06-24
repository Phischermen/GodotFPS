using Godot;
using System;

public class Player : KinematicBody
{
    private Vector2 CameraAngle;

    [Export]
    public bool InvertX = false;
    [Export]
    public bool InvertY = false;
    [Export]
    public float SensitivityX = 0.3f;
    [Export]
    public float SensitivityY = 0.3f;
    [Export]
    public int PitchMin = -89;
    [Export]
    public int PitchMax = 89;

    [Export]
    public float FullSpeedFly = 1f;
    [Export]
    public float AccelerationFly = 4f;

    [Export]
    public float Gravity = -18.16f;
    [Export]
    public float FullSpeedWalk = 10f;
    [Export]
    public float FullSpeedSprint = 20f;
    [Export]
    public float AccelerationWalk = 4f;
    [Export]
    public float DeaccelerationWalk = 6f;
    [Export]
    public float AccelerationAir = 2f;
    [Export]
    public float DeaccelerationAir = 1f;

    [Export]
    public float JumpHeight = 9f;
    [Export]
    public bool BunnyHopping = false;

    [Export]
    public float MaxSlope = 30;

    public bool Flying = false;
    public Vector3 Velocity;
    public Vector3 AirVelocity;
    public Vector3 Direction;

    private Spatial _Head;
    private Camera _Camera;
    private AnimationTree _AnimationTree;
    private AnimationNodeStateMachinePlayback StateMachine;
    private float Walk1d;

    private JumpArea _JumpArea;
    private SnapArea _SnapArea;

    private RayCast GroundRay;
    private RayCast[] LedgeRays;
    private Label ClimbLabel;

    public override void _Ready()
    {
        _Head = GetNode<Spatial>(new NodePath("Head"));
        _Camera = GetNode<Camera>(new NodePath("Head/CameraWrapper/Camera"));
        _AnimationTree = GetNode<AnimationTree>(new NodePath("AnimationTree"));
        StateMachine = (AnimationNodeStateMachinePlayback)_AnimationTree.Get("parameters/playback");
        Walk1d = (float)_AnimationTree.Get("parameters/Walk1d/blend_position");

        _JumpArea = GetNode<JumpArea>(new NodePath("JumpArea"));
        _SnapArea = GetNode<SnapArea>(new NodePath("SnapArea"));
        GroundRay = GetNode<RayCast>(new NodePath("GroundRay"));
        LedgeRays = new RayCast[GetNode<Node>(new NodePath("Head/LedgeRays")).GetChildCount()];
        for(int i = 0; i < LedgeRays.Length; ++i)
        {
            LedgeRays[i] = GetNode<RayCast>(new NodePath("Head/LedgeRays/LedgeRay" + (i + 1)));
        }
        ClimbLabel = GetNode<Label>(new NodePath("UI/ClimbLabel"));
        MaxSlope = Mathf.Deg2Rad(MaxSlope);

        //Set animation
        _AnimationTree.Active = true;
        StateMachine.Start("Walk1d");
    }

    public override void _PhysicsProcess(float delta)
    {
        if (Flying)
        {
            Fly(delta);
        }
        else
        {
            Walk(delta);
        }
        if (Input.IsActionJustPressed("toggle_fly"))
        {
            Flying = !Flying;
            Velocity = Vector3.Zero;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion){
            //Downcast event to mouse motion
            InputEventMouseMotion mouseMotionEvent = (InputEventMouseMotion)@event;
            //Update camera angle
            CameraAngle.x = CameraAngle.x + mouseMotionEvent.Relative.x * SensitivityX;
            CameraAngle.y = Mathf.Clamp(CameraAngle.y + mouseMotionEvent.Relative.y * SensitivityY, PitchMin, PitchMax);
            //Apply rotation
            _Head.SetRotationDegrees(new Vector3(0, (InvertX ? 1 : -1)*CameraAngle.x, 0));
            _Camera.SetRotationDegrees(new Vector3((InvertY ? 1 : -1)*CameraAngle.y, 0, 0));
            //Tell event was handled
            GetTree().SetInputAsHandled();
        }
    }

    private void Fly(float delta)
    {
        //Reset direction
        Direction = Vector3.Zero;

        //Get the rotation of the camera
        Basis aim = _Camera.GetGlobalTransform().basis;

        //Get input and set direction
        if (Input.IsActionPressed("move_forward") )
        {
            Direction -= aim.z;
        }
        if (Input.IsActionPressed("move_backward"))
        {
            Direction += aim.z;
        }
        if (Input.IsActionPressed("move_left"))
        {
            Direction -= aim.x;
        }
        if (Input.IsActionPressed("move_right"))
        {
            Direction += aim.x;
        }
        Direction = Direction.Normalized();

        //Set target speed
        Vector3 target = Direction * FullSpeedFly;

        //Calculate velocity
        Velocity = Velocity.LinearInterpolate(target, AccelerationFly * delta);
        
        //Move Player
        Translate(Velocity);
    }

    private void Walk(float delta)
    {

        //Get the rotation of the camera
        Basis aim = _Head.GetGlobalTransform().basis;

        //Get slope of floor 
        Vector3 normal;
        if (IsOnFloor())
        {
            normal = GroundRay.GetCollisionNormal();
        }
        else
        {
            normal = Vector3.Up;
        }

        //Set aim as to remove y component and apply slope
//        aim.Column0 = new Vector3(aim.x.x + (normal.Abs() * aim.x).x, 0, aim.x.z + (normal.Abs() * aim.x).z).Normalized();
//        aim.Column1 = Vector3.Zero;
//        aim.Column2 = new Vector3(aim.z.x + (normal.Abs() * aim.z).x, 0, aim.z.z + (normal.Abs() * aim.z).z).Normalized();
        
        //Calculate ground angle
        float groundAngle = Mathf.Acos(normal.Dot(Vector3.Up));

        //Determine if slipping and set initial direction
        bool slipping = false;
        if (groundAngle > MaxSlope)
        {
            slipping = true;
            Direction = Vector3.Up.Slide(normal);
        }
        else
        {
            //if (IsOnFloor())
            //{
                Direction = Vector3.Zero;
            //}
        }

        //Get input and set direction
        bool userMoving = false;
        if (Input.IsActionPressed("move_forward"))
        {
            Direction -= aim.z * (slipping ? -1 : 1);
            userMoving = true;
        }
        if (Input.IsActionPressed("move_backward"))
        {
            Direction += aim.z * (slipping ? -1 : 1);
            userMoving = true;
        }
        if (Input.IsActionPressed("move_left"))
        {
            Direction -= aim.x * (slipping ? -1 : 1);
            userMoving = true;
        }
        if (Input.IsActionPressed("move_right"))
        {
            Direction += aim.x * (slipping ? -1 : 1);
            userMoving = true;
        }
        Direction = Direction.Normalized(); 
        
        Vector3 velocityNoGravity = new Vector3(Velocity.x, 0, Velocity.z);

        //Get speed and acceleration dependant variables
        bool sprint = Input.IsActionPressed("sprint");
        bool accelerate = Direction.Dot(velocityNoGravity) > 0;

        //Set target speed
        Vector3 target;
        if (slipping)
        {
            target = Direction * Gravity * Vector3.Up.Dot(normal);
        }
        else
        {
            target = Direction * (sprint ? FullSpeedSprint : FullSpeedWalk);
        }
        

        //Apply gravity
        Velocity.y += Gravity * delta;

        //Calculate velocity
        velocityNoGravity = velocityNoGravity.LinearInterpolate(target, (accelerate ? (IsOnFloor() ? AccelerationWalk: AccelerationAir) : (IsOnFloor() ? DeaccelerationWalk : DeaccelerationAir)) * delta);
        Velocity.x = velocityNoGravity.x;
        Velocity.z = velocityNoGravity.z;

        //Set animation blend
        if (userMoving)
        {
            Walk1d = Mathf.Min(1, Walk1d + (sprint ? 0.01f : 0.04f));
        }
        else
        {
            Walk1d = Mathf.Max(0, Walk1d - 0.1f);
        }
        _AnimationTree.Set("parameters/Walk1d/blend_position", Walk1d);

        //Get jump
        if (BunnyHopping ? Input.IsActionPressed("jump") && _JumpArea.Bodies > 0 && _JumpArea.CanBunnyHop : Input.IsActionJustPressed("jump") && _JumpArea.Bodies > 0)
        {
            Velocity.y = 0;
            Velocity += normal*JumpHeight;
            _JumpArea.CanBunnyHop = false;
            _SnapArea.Snap = false;
            //GD.Print(_SnapArea.Snap);
        }

        //Calculate ledge point
        Vector3 ClosestLedgePoint = Vector3.Inf;
        bool canClimb = false;
        ClimbLabel.Visible = false;
        foreach(RayCast LedgeRay in LedgeRays)
        {
            if (LedgeRay.IsColliding())
            {
                if (LedgeRay.GetCollisionPoint().DistanceTo(Translation) < ClosestLedgePoint.DistanceTo(Translation))
                {
                    ClosestLedgePoint = LedgeRay.GetCollisionPoint();
                    canClimb = true;
                }
            }
        }
        ClimbLabel.Visible = canClimb;
        if (Input.IsActionJustPressed("climb") && canClimb)
        {
            SetTranslation(ClosestLedgePoint + Vector3.Up);
        }

        //Move Player
        if (userMoving || slipping)
        {
            Velocity = MoveAndSlideWithSnap(Velocity, _SnapArea.Snap ? new Vector3(0, -1f, 0) : Vector3.Zero, Vector3.Up, true, 4, 1.0472f/*60*/);
        }
        else
        {
            Velocity = MoveAndSlide(Velocity, Vector3.Up, true, 4, 1.0472f/*60*/);
        }
    }
    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}