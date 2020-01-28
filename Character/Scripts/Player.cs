using Godot;
using Godot.Collections;
//using System;
public sealed class Player : KinematicBody, ISave
{
    private Player() { }
    private static Player _singleton;
    public static Player Singleton
    {
        get { return _singleton; }
        private set
        {
            SI.SetAndWarnUser(ref value, ref _singleton);
        }
    }

    public bool Imobile;
    private float Hinput;
    private float Vinput;
    private float Linput;
    private float Xinput;
    private float Yinput;
    private bool[] InputDoubleChecker = new bool[6];
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
    public float JoySensitivityX = 4f;
    [Export]
    public float JoySensitivityY = 4f;
    public int PitchMin = -89;
    public int PitchMax = 89;
    private readonly float PointerRiseMultiplier = 2f;

    public float FullSpeedFly = 1f;
    public float AccelerationFly = 4f;

    public float Gravity = -29.57f;
    public float FullSpeedWalk = 10f;
    public float FullSpeedSprint = 17f;
    public float AccelerationWalk = 4f;
    public float DeaccelerationWalk = 8f;
    public float AccelerationAir = 2f;
    public float DeaccelerationAir = 1f;

    public float ClimbSpeed = 0.2f;

    public float JumpHeight = 10f;
    [Export]
    public int JumpCount = 1;
	private int _jumpsLeft = 0;
    public int JumpsLeft
	{
		get{return _jumpsLeft;}
		set
		{
			_jumpsLeft = value;
			JumpLabel.Text = "Jumps Left: " + _jumpsLeft;
		}
	}
	
    [Export]
    public bool BunnyHopping = false;
    [Export]
    public bool CanCrouchJump = true;
    public bool CrouchJumped;

    private readonly float HeadDipThreshold = 13f;
    private readonly float FatalFallVelocity = 50f;
    private readonly float MajorInjuryFallVelocity = 40f;
    private readonly float MinorInjuryFallVelocity = 30f;

    [Export]
    private bool _headBobbing = true;
    public bool HeadBobbing
    {
        get{return _headBobbing;}
        set
        {
            if(!value) PlayerAnimationTree.Singleton.HeadBobBlend = 0f;
            _headBobbing = value;
        }
    }
    private readonly float BobIncrementWalk = 2f;
    private readonly float BobIncrementSprint = 3f;
    private readonly float BobDecrementFloor = 4f;
    private readonly float BobDecrementAir = 5f;
    Vector2 SwingTarget;

    [Export]
    public float MaxSlopeSlip = 30f;
    [Export]
    public float MaxSlopeNoWalk = 44f;

    public bool Flying = false;
    public bool Climbing = false;
    public Vector3 Velocity;
    public float ImpactVelocity;
    public Vector3 Direction;

    private bool WasInAir = false;

    private bool _crouched;
    public bool Crouched
    {
        get{return _crouched;} 
        set
        {
            if(value)
            {
                PlayerAnimationTree.Singleton.StateMachineCrouch.Travel("Crouch");
            }
            else
            {
                PlayerAnimationTree.Singleton.StateMachineCrouch.Travel("Uncrouch");
            }
            //Enable correct collision shape
            _CollisionStand.Disabled = value;
            _CollisionCrouch.Disabled = !value;

            //Enable/Disable StandArea
            StandArea.Monitoring = value;
            _crouched = value;
        }
    }
    private bool WantsToUncrouch = false;
    
    [Export]
    public bool CrouchToggle;
    
    [Export]
    public bool SprintToggle;
    [Export]
    public bool SprintToWalk;
    
    private bool Sprint;
    
    private CollisionShape _CollisionStand;
    private CollisionShape _CollisionCrouch;
    private Array<CollisionShape> OtherCollision;
    private float StandHeight;
    private float CrouchHeight;

    private Area StandArea;

    private Spatial Head;
    private Spatial Neck;
    private Camera _Camera;
    private Spatial ArmWrapper;
    private Camera HighlightCamera;
    private Timer ClimbTimer;

    //private PlayerFeet _JumpArea;
    private SnapArea _SnapArea;

    private RayCast ScanRay;
    private RayCast PointerProjectRay;
    [Export]
    public float RetractThreshold = 2.5f;
    [Export]
    public float InteractThreshold = 5f;
    [Export]
    public float ScanTimeout;
    public Node ScannedNode;
    public Interactable FocusedInteractable;
    private AudioStreamPlayer ScanPlayer;
    private Timer ScanTimer;
    private bool ScanIsClear = true;
    private bool WantsToClearScan = false;

    private RayCast GroundRay;
    private RayCast[] LedgeRays;
    private Label ClimbLabel;
    private Label JumpLabel;
	[Export]
	public float ClimbTimeout;
	private bool WantsToClimb;
    private Vector3 ClimbPoint;
    private Vector3 ClimbStep;
    private float ClimbDistance;

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
        _CollisionStand = GetNode<CollisionShape>("CollisionStand");
        _CollisionCrouch = GetNode<CollisionShape>("CollisionCrouch");
        CapsuleShape shape1 = (CapsuleShape)_CollisionStand.Shape;
        CapsuleShape shape2 = (CapsuleShape)_CollisionCrouch.Shape;
        StandHeight = shape1.Height;
        CrouchHeight = shape2.Height;
        OtherCollision = NX.FindAll<CollisionShape>(this, true);
        OtherCollision.Remove(_CollisionStand);
        OtherCollision.Remove(_CollisionCrouch);

        StandArea = GetNode<Area>("StandArea");
        Head = GetNode<Spatial>("Head");
        Neck = GetNode<Spatial>("Head/Wrapper1/Neck");
        _Camera = GetNode<Camera>("Head/Wrapper1/Neck/Wrapper2/Wrapper3/Camera");
        HighlightCamera = GetNode<Camera>("Head/Wrapper1/Neck/Wrapper2/Wrapper3/Camera/Viewport2/Camera");
        ArmWrapper = GetNode<Spatial>("Head/Wrapper1/Neck/Wrapper2/Wrapper3/Camera/Viewport/Wrapper4");
        ClimbTimer = GetNode<Timer>("ClimbTimer");
		ClimbTimer.Connect("timeout", this, "_OnClimbTimerTimeout");
       
        //_JumpArea = GetNode<PlayerFeet>("JumpArea");
        _SnapArea = GetNode<SnapArea>("SnapArea");
        GroundRay = GetNode<RayCast>("GroundRay");
        LedgeRays = new RayCast[GetNode<Node>(("Head/LedgeRays")).GetChildCount()];
        for(int i = 0; i < LedgeRays.Length; ++i)
        {
            LedgeRays[i] = GetNode<RayCast>("Head/LedgeRays/LedgeRay" + (i + 1));
        }
        ClimbLabel = GetNode<Label>("UI/Reticle/ClimbLabel");
        JumpLabel = GetNode<Label>("UI/JumpLabel");
		
		ScanRay = GetNode<RayCast>("Head/Wrapper1/Neck/ScanRay");
		PointerProjectRay = GetNode<RayCast>("Head/Wrapper1/Neck/PointerProjectRay");
        ScanPlayer = ScanRay.GetNode<AudioStreamPlayer>("Player");
        ScanTimer = GetNode<Timer>("ScanTimer");
        ScanTimer.Connect("timeout", this, "_OnScanTimerTimeout");

        //Check for vital actions in InputMap
        IX.CheckAndAddAction("move_forward", KeyList.W);
        IX.CheckAndAddAction("move_backward", KeyList.S);
        IX.CheckAndAddAction("move_left", KeyList.A);
        IX.CheckAndAddAction("move_right", KeyList.D);
        IX.CheckAndAddAction("move_up", KeyList.E);
        IX.CheckAndAddAction("move_down", KeyList.Q);
        IX.CheckAndAddAction("toggle_fly", KeyList.V);
        IX.CheckAndAddAction("interact", KeyList.E);
        IX.CheckAndAddAction("drop", KeyList.R);
        IX.CheckAndAddAction("release_mouse", KeyList.F1, false, false, true);
        IX.CheckAndAddAction("place_debug_camera", KeyList.F2, false, false, true);

        Input.Singleton.Connect("joy_connection_changed", this, "_OnJoyConnectionChanged");

        MaxSlopeSlip = Mathf.Deg2Rad(MaxSlopeSlip);
        MaxSlopeNoWalk = Mathf.Deg2Rad(MaxSlopeNoWalk);

        //Set jumps left
        JumpsLeft = JumpCount;

        //Set animation
        HeadBobbing = _headBobbing;
        SwingTarget = Vector2.Zero;
        
        //Capture mouse
        Input.SetMouseMode(Input.MouseMode.Captured);

        StandArea.Monitoring = true;
    }

    public override void _PhysicsProcess(float delta)
    {
        GroundRay.Call("update", 0.1f + (-Velocity.y * delta));
		ArmWrapper.GlobalTransform = _Camera.GlobalTransform;
		

        //Joypad Camera movement
        if(Xinput != 0f || Yinput != 0f)
        {
            MoveCamera(Xinput, Yinput, JoySensitivityX, JoySensitivityY);
            ApplyCameraAngle();
        }
        
		PlayerArms.Singleton.Retract = Translation.DistanceTo(ScanRay.GetCollisionPoint()) < RetractThreshold;
        if (!Imobile)
        {
            if (Flying)
            {
                Fly(delta);
            }
            else if (Climbing)
            {
                Climb(delta);
            }
            else
            {
                Walk(delta);
                ScanAndInteract();
                if (Input.IsActionJustPressed("drop"))
                {
                    LiftSystem.Drop();
                }
            }
        }
        if (Input.IsActionJustPressed("toggle_fly"))
        {
            Flying = !Flying;
            Velocity = Vector3.Zero;
            PlayerAnimationTree.Singleton.Active = !Flying;
        }
        HighlightCamera.GlobalTransform = _Camera.GlobalTransform;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey)
        {
            //Downcast to key
            InputEventKey eventKey = (InputEventKey)@event;

            //Update Motion Input
            if (!Imobile)
            {
                if (eventKey.IsAction("move_forward")) SetMotionParameterAndConsumeInput(ref Vinput, false, eventKey, 0);
                else if (eventKey.IsAction("move_backward")) SetMotionParameterAndConsumeInput(ref Vinput, true, eventKey, 1);
                else if (eventKey.IsAction("move_left")) SetMotionParameterAndConsumeInput(ref Hinput, false, eventKey, 2);
                else if (eventKey.IsAction("move_right")) SetMotionParameterAndConsumeInput(ref Hinput, true, eventKey, 3);
                else if (eventKey.IsAction("move_up")) SetMotionParameterAndConsumeInput(ref Linput, true, eventKey, 4);
                else if (eventKey.IsAction("move_down")) SetMotionParameterAndConsumeInput(ref Linput, false, eventKey, 5);
            }
            if (eventKey.IsAction("release_mouse") && eventKey.Pressed)
            {
                Input.SetMouseMode(Input.GetMouseMode() == Input.MouseMode.Captured ? Input.MouseMode.Visible : Input.MouseMode.Captured);
            }
        }
        else if(@event is InputEventJoypadMotion)
        {
            //Downcast to motion
            InputEventJoypadMotion eventMotion = (InputEventJoypadMotion)@event;

            if(eventMotion.Axis == (int)JoystickList.Axis0 || eventMotion.Axis == (int)JoystickList.Axis1)
            {
                //Update Motion Input
                if (!Imobile)
                {
                    if (eventMotion.IsAction("move_forward")) SetMotionParameterAndConsumeInput(ref Vinput, eventMotion);
                    else if (eventMotion.IsAction("move_backward")) SetMotionParameterAndConsumeInput(ref Vinput, eventMotion);
                    else if (eventMotion.IsAction("move_left")) SetMotionParameterAndConsumeInput(ref Hinput, eventMotion);
                    else if (eventMotion.IsAction("move_right")) SetMotionParameterAndConsumeInput(ref Hinput, eventMotion);
                    else if (eventMotion.IsAction("move_up")) SetMotionParameterAndConsumeInput(ref Linput, eventMotion);
                    else if (eventMotion.IsAction("move_down")) SetMotionParameterAndConsumeInput(ref Linput, eventMotion);
                }
            }
            else if (eventMotion.Axis == (int)JoystickList.Axis2)
            {
                SetMotionParameterAndConsumeInput(ref Xinput, eventMotion);
            }
            else if(eventMotion.Axis == (int)JoystickList.Axis3)
            {
                SetMotionParameterAndConsumeInput(ref Yinput, eventMotion);
            }
            
        }
        else if (@event is InputEventMouseMotion && Input.GetMouseMode() == Input.MouseMode.Captured)
        {
            //Downcast to mouse motion
            InputEventMouseMotion eventMouseMotion = (InputEventMouseMotion)@event;

            //Update camera angle
            MoveCamera(eventMouseMotion.Relative, SensitivityX, SensitivityY);

            //Apply rotation
            ApplyCameraAngle();
            
            //Tell event was handled
            GetTree().SetInputAsHandled();
        }
    }

    private void MoveCamera(Vector2 xy, float sensitivityX, float sensitivityY)
    {
        SwingTarget += xy * new Vector2(1f, -1f) * PlayerArms.SwingIncrement;
        SwingTarget = SwingTarget.Clamped(1f);
        CameraAngle.x += xy.x * sensitivityX;
        CameraAngle.y += xy.y * sensitivityY;
    }

    private void MoveCamera(float x, float y, float sensitivityX, float sensitivityY)
    {
        SwingTarget.x += x * PlayerArms.SwingIncrement;
        SwingTarget.y += y * -1f * PlayerArms.SwingIncrement;
        SwingTarget = SwingTarget.Clamped(1f);
        CameraAngle.x += x * sensitivityX;
        CameraAngle.y += y * sensitivityY;
    }

    public void LookAt(Vector3 point)
    {
        Vector3 gt = _Camera.GetGlobalTransform().origin;
        Vector3 point2 = new Vector3(point.x, gt.y, point.z);
        float x = gt.z - point.z;
        float y = gt.y - point.y;
        float r1 = gt.DistanceTo(point2);
        float r2 = gt.DistanceTo(point);
        CameraAngle.x = Mathf.Rad2Deg(Mathf.Acos(x / r1));
        CameraAngle.y = Mathf.Rad2Deg(Mathf.Asin(y / r2));
        ApplyCameraAngle();
    }

    private void ApplyCameraAngle()
    {
        //Clamp angle
        CameraAngle.y = Mathf.Clamp(CameraAngle.y, PitchMin, PitchMax);

        //Adjust Scanray if Player looks upwards
        float xAngle = (InvertX ? 1 : -1) * CameraAngle.x;
        float yAngle = (InvertY ? 1 : -1) * CameraAngle.y;
        if (yAngle > 0) ScanRay.Translation = _Camera.Transform.basis.y * Mathf.Min(1f, PointerRiseMultiplier * (yAngle / PitchMax));
        Head.RotationDegrees = new Vector3(0, xAngle, 0);
        Neck.RotationDegrees = new Vector3(yAngle, 0, 0);
    }

    private void SetMotionParameterAndConsumeInput(ref float parameter, bool positive, InputEventKey inputEvent, int doubleCheckIndex)
    {
        GetTree().SetInputAsHandled();
        if (inputEvent.Echo) return;

        //Ensure player does not get stuck walking in one direction
        if (inputEvent.Pressed == InputDoubleChecker[doubleCheckIndex]) return;
        InputDoubleChecker[doubleCheckIndex] = inputEvent.Pressed;
        parameter += ((inputEvent.Pressed == positive) ? 1f : -1f);

        //Tell Kevin he sucks and can't write working code
        if (parameter > 1f || parameter < -1f)
            GD.Print("KEVIN YOUR INPUT IS STILL BROKEN!");
    }

    private void SetMotionParameterAndConsumeInput(ref float parameter, InputEventJoypadMotion inputEvent)
    {
        GetTree().SetInputAsHandled();
        if(Mathf.Abs(inputEvent.AxisValue) > 0.1f)
        {
            parameter = inputEvent.AxisValue;
        }
        else
        {
            parameter = 0;
        }
        
    }

    private void _OnJoyConnectionChanged(int index, bool connected)
    {
        if(index == 0 && connected == false)
        {
            GD.Print("Success");
            Hinput = Vinput = Linput = Xinput = Yinput = 0f;
            for(int i = 0; i < InputDoubleChecker.Length; ++i)
            {
                InputDoubleChecker[i] = false;
            }
        }
    }

    private void Fly(float delta)
    {
        //Reset direction
        Direction = Vector3.Zero;

        //Get the rotation of the head
        Basis aim = _Camera.GlobalTransform.basis;

        //Get input and set direction
        Direction += aim.z * Vinput;
        Direction += aim.x * Hinput;
        Direction += aim.y * Linput;
        Direction = Direction.Normalized();

        //Set target speed
        Vector3 target = Direction * FullSpeedFly;
        
        //Calculate velocity
        Velocity = Velocity.LinearInterpolate(target, AccelerationFly * delta);

        //Move Player
        //Translate(Velocity);
        GlobalTranslate(Velocity);
    }

    private void Walk(float delta)
    {
        //Since IsOnFloor() doesn't properly update on concave collision shapes, count the number of bodies that _JumpArea is colliding with
       

        //Get the rotation of the head
        Basis aim = Head.GlobalTransform.basis;

        //Get slope of floor and if ground hit, set head velocity
        Vector3 normal;
        bool playerFeetOverlapsFloor = PlayerFeet.Singleton.Bodies > 0;
        if (playerFeetOverlapsFloor)
        {
            normal = GroundRay.GetCollisionNormal();
        }
        else
        {
            normal = Vector3.Up;
        }
        
        //Calculate ground angle
        float groundAngle = Mathf.Acos(normal.Dot(Vector3.Up));

        //Determine if slipping and set initial direction
        bool slipping = false;
        bool noWalkSlipping = false;
        if (groundAngle > MaxSlopeSlip)
        {
            slipping = true;
            if(groundAngle > MaxSlopeNoWalk)
            {
                noWalkSlipping = true;
            }
            Direction = Vector3.Up.Slide(normal);
        }
        else
        {
                Direction = Vector3.Zero;
        }

        //Get crouch
        bool crouchPressed = CrouchToggle && !CrouchJumped ? Input.IsActionJustPressed("crouch") : (Input.IsActionPressed("crouch") != Crouched);

        //Determine if landing
        PlayerArms.Singleton.Land = false;
        if(IsOnFloor()){
            if (WasInAir)
            {
                //GD.Print("Landed");
                //Reset number of jumps
                JumpsLeft = JumpCount;

                //Play land animations
                PlayerArms.Singleton.Fall = false;
                PlayerArms.Singleton.Land = true;
                if (!slipping && ImpactVelocity >= HeadDipThreshold)
                {
                    PlayerAnimationTree.Singleton.LandBlend = Mathf.Min(1f, Mathf.Abs((ImpactVelocity - HeadDipThreshold) / (-Gravity - HeadDipThreshold)));
                    PlayerAnimationTree.Singleton.Land();
                }

                //Play Land Sounds if Velocity is significant
                PlayerFeet.Singleton.LandPlayer.Play();

                //GD.Print(ImpactVelocity);
                //Deal fall damage
                if (ImpactVelocity >= FatalFallVelocity)
                {
                    PlayerHealthManager.Singleton.Kill("A Fatal Fall");
                }
                else if(ImpactVelocity >= MajorInjuryFallVelocity)
                {
                    PlayerHealthManager.Singleton.TakeDamage("A Major Spill", 50);
                }
                else if(ImpactVelocity >= MinorInjuryFallVelocity)
                {
                    PlayerHealthManager.Singleton.TakeDamage("A Minor Spill", 10);
                }

                //Determine if player had crouch jumped
                if (CrouchJumped)
                {
                    CrouchJumped = false;
                    WantsToUncrouch = crouchPressed;
                }

                WasInAir = false;
            }
        }
        else if (Mathf.Abs(Velocity.y) > 4f)
        {
			//GD.Print(Velocity.y);
            WasInAir = true;
            PlayerArms.Singleton.Fall = true;
            ImpactVelocity = -Velocity.y;
        }
        
        //Get input and set direction
        bool userMoving = (Vinput != 0 || Hinput != 0);
        Direction += aim.z * (slipping ? -1f : 1f) * Vinput;
        Direction += aim.x * (slipping ? -1f : 1f) * Hinput;
        if(Direction.Length() > 1f) Direction = Direction.Normalized();
        
        Vector3 velocityNoGravity = new Vector3(Velocity.x, 0, Velocity.z);

        //Get sprint
        if (SprintToggle)
        {
            if (Input.IsActionJustPressed("sprint"))
            {
                Sprint = !Sprint;
            }
        }
        else
        {
             Sprint = (Input.IsActionPressed("sprint") != SprintToWalk);
        }
        
        //Get speed and acceleration dependant variables
        bool accelerate = Direction.Dot(velocityNoGravity) > 0;

        //Set target speed
        Vector3 targetVelocity;
        if (slipping)
        {
            targetVelocity = Direction * Gravity * (1f - Vector3.Up.Dot(normal));
        }
        else
        {
            targetVelocity = Direction * (Sprint ? (IsOnFloor() ? FullSpeedSprint : Mathf.Max(Velocity.Length(), FullSpeedWalk)) : FullSpeedWalk);
        }

        //Apply gravity
        Velocity.y += Gravity * delta;
        

        //Calculate velocity
        float acceleration = IsOnFloor() || (IsOnWall() && noWalkSlipping) ? AccelerationWalk: AccelerationAir;
        float deacceleration =  IsOnFloor() ? DeaccelerationWalk : DeaccelerationAir;
        velocityNoGravity = velocityNoGravity.LinearInterpolate(targetVelocity, (accelerate ? acceleration : deacceleration) * delta);
        Velocity.x = velocityNoGravity.x;
        Velocity.z = velocityNoGravity.z;

        //Set walking animation blend
        float bob1D = PlayerArms.Singleton.Walk;
        if (userMoving && IsOnFloor())
        {
            bob1D = Mathf.Min(1f, bob1D + ((Sprint ? BobIncrementSprint : BobIncrementWalk) * delta));
        }
        else
        {
            bob1D = Mathf.Max(0.01f, bob1D - ((IsOnFloor() ? BobDecrementFloor : BobDecrementAir) * delta));
        }
        if(HeadBobbing){
            PlayerAnimationTree.Singleton.HeadBobBlend = bob1D;
        }
        PlayerArms.Singleton.Walk = bob1D;
        PlayerAnimationTree.Singleton.HeadBobScale = Sprint ? 1.5f : 1f;

        //Set arm swing blend
        SwingTarget = SwingTarget.LinearInterpolate(Vector2.Zero, 0.05f);
        PlayerArms.Singleton.Swing = SwingTarget;

        //Set step timer
        if(bob1D > 0.1f)
        {
            if(PlayerFeet.Singleton.StepTimer.Paused == true)
            {
                //GD.Print("Not Paused");
                PlayerFeet.Singleton.StepTimer.Paused = false;
            }
        }
        else 
        {
            if (PlayerFeet.Singleton.StepTimer.Paused == false)
            {
                //GD.Print("Paused");
                PlayerFeet.Singleton.StepTimer.Paused = true;
            }
        }
        PlayerFeet.Singleton.StepTimer.WaitTime = Sprint ? 0.25f : 0.5f;

        //Get jump
        PlayerArms.Singleton.Jump = false;
        if ((BunnyHopping ? Input.IsActionPressed("jump") : Input.IsActionJustPressed("jump")) && (playerFeetOverlapsFloor) || Input.IsActionJustPressed("jump") && JumpsLeft > 0)
        {
            PlayerArms.Singleton.Jump = true;
            if(!PlayerFeet.Singleton.JumpPlayer.IsPlaying()) PlayerFeet.Singleton.JumpPlayer.Play();
            Velocity.y = 0f;
            //Velocity += normal * JumpHeight;
            //Velocity.y *= (noWalkSlipping ? 0.1f : 1f);
            if (!slipping && Direction.Dot(velocityNoGravity) < 0f)
            {
                //GD.Print("Reset Velocity");
                Velocity.x = 0f;
                Velocity.z = 0f;
            }
            Velocity += (noWalkSlipping ? normal : Vector3.Up) * JumpHeight; 
            PlayerFeet.Singleton.CanBunnyHop = false;
            _SnapArea.Snap = false;

            if (!playerFeetOverlapsFloor)
            {
                //Update number of jumps
                JumpsLeft -= 1;
            }
        }

        if (crouchPressed || (Crouched && WantsToUncrouch))
        {
            if (IsOnFloor())
            {
                //Check if uncrouch is possible
                if ((Crouched && StandArea.GetOverlappingBodies().Count == 0) || (!Crouched))
                {
                    //Invert crouch (animation, collision, etc. handled in get set)
                    Crouched = !Crouched;
                    WantsToUncrouch = false;
                }
                else if (crouchPressed)
                {
                    //Invert WantToUncrouch. Player will uncrouch at latest opportunity.
                    WantsToUncrouch = !WantsToUncrouch;
                }
            }
            else if(CanCrouchJump && !Crouched && !CrouchJumped)
            {
                CrouchJumped = true;
                Crouched = true;
                PlayerAnimationTree.Singleton.StateMachineCrouch.Start("CrouchJump");
                Translate(Vector3.Up * 1.1f);
                //GD.Print("Crouch jump.");
            }
        }
		
		//Get Climb
		if (Input.IsActionPressed("climb"))
		{
			WantsToClimb = true;
			ClimbTimer.Start(ClimbTimeout);
		}
        
        //Calculate ledge point
        Vector3 closestLedgePoint = Vector3.Inf;
        bool canClimb = false;
        bool withCrouch = false;
        ClimbLabel.Visible = false;
        if(!Crouched)
        {
            foreach(RayCast LedgeRay in LedgeRays)
            {
                if (LedgeRay.IsColliding())
                {
                    if (LedgeRay.GetCollisionPoint().DistanceTo(Translation) < closestLedgePoint.DistanceTo(Translation))
                    {
                        closestLedgePoint = LedgeRay.GetCollisionPoint();
                        canClimb = true;
                    }
                }
            }
            closestLedgePoint = closestLedgePoint + (Vector3.Up * (StandHeight + 0.075f));

            if (canClimb)
            {
                //Disable all other colliders
                bool[] shapeDisabledState = new bool[OtherCollision.Count];
                for (int i = 0; i < OtherCollision.Count; ++i)
                {
                    //GD.Print("Disabling collision ", OtherCollision[i].Name);
                    shapeDisabledState[i] = OtherCollision[i].Disabled;
                    OtherCollision[i].Disabled = true;
                }

                //Test to see if player has space to climb
                canClimb = !TestMove(new Transform(Quat.Identity, closestLedgePoint), Vector3.Zero);
                if (!canClimb)
                {
                    _CollisionStand.Disabled = true;
                    _CollisionCrouch.Disabled = false;
                    canClimb = !TestMove(new Transform(Quat.Identity, closestLedgePoint), Vector3.Zero);
                    withCrouch = canClimb;
                    _CollisionStand.Disabled = false;
                    _CollisionCrouch.Disabled = true;
                }

                //Reset colliders
                for (int i = 0; i < OtherCollision.Count; ++i)
                {
                    //GD.Print("Reseting other collision", OtherCollision[i], " ", shapeDisabledState[i]);
                    OtherCollision[i].Disabled = shapeDisabledState[i];
                }
            }
            
            ClimbLabel.Visible = canClimb;
            if (canClimb && WantsToClimb)
            {
                if (withCrouch)
                {
                    Crouched = true;
                }

                //Set climb point and enter climb state
                ClimbPoint = closestLedgePoint;
                ClimbStep = (ClimbPoint - Translation).Normalized() * ClimbSpeed;
                ClimbDistance = (ClimbPoint - Translation).Length();
                PlayerAnimationTree.Singleton.Climb();
                PlayerAnimationTree.Singleton.HeadBobScale = 0f;
                Climbing = true;

                //Reset ImpactVelocity and zero y Velocity to avoid confusing animations
                ImpactVelocity = Velocity.y = 0f;
            }
        }
        
        //Move Player
        if (userMoving || slipping)
        {
            Velocity = MoveAndSlideWithSnap(Velocity, _SnapArea.Snap ? new Vector3(0, -1f, 0) : Vector3.Zero, Vector3.Up, true, 4, MaxSlopeNoWalk);
        }
        else
        {
            Velocity = MoveAndSlide(Velocity, Vector3.Up, true, 4, MaxSlopeNoWalk);
        }
    }
	
	private void _OnClimbTimerTimeout()
	{
		WantsToClimb = false;
	}

    private void Climb(float delta){
        Translation = Translation + ClimbStep;
        ClimbDistance -= ClimbSpeed;
        if(ClimbDistance <= 0)
        {
			//Reset jumps in case one was spent to initiate climb
			JumpsLeft = JumpCount;
            Translation = ClimbPoint;
            Climbing = false;
            ClimbTimer.Set("wants_to_climb",false);
            PlayerAnimationTree.Singleton.HeadBobScale = 1f;
        }
    }

    private void _OnScanTimerTimeout()
    {
        GD.Print("Timeout");
        //Double checks that user still wants to clear scan and that FocusedInteractable exists
        PlayerReticle.PointingAtInteractable = false;
        FocusedInteractable.Highlight = false;
        FocusedInteractable = null;
        ScanIsClear = true;
    }

    private void UpdatePointerProjectRay(Vector3 destination)
    {
        //Make ray start at player's reticle and end at the global "destination" point
        //Vector3 lorigin = PointerProjectRay.ToLocal(_Camera.ProjectRayOrigin(PlayerReticle.Singleton.RectPosition));
        //Vector3 ldestination = PointerProjectRay.ToLocal(destination);
        //PointerProjectRay.Translation = lorigin;
        //PointerProjectRay.CastTo = ldestination;
        Vector3 origin = _Camera.ProjectRayOrigin(PlayerReticle.GetPointerPosition());
        Vector3 normal = _Camera.ProjectLocalRayNormal(PlayerReticle.GetPointerPosition());
        Transform update = new Transform(_Camera.GlobalTransform.basis, origin);
        PointerProjectRay.GlobalTransform = update;
        PointerProjectRay.CastTo = normal * 10f;
    }

    private void ScanAndInteract()
    {
        //Position Reticle at proper point and get collider
        Node node;
        Vector3 position;
        if (ScanRay.IsColliding())
        {
            position = ScanRay.GetCollisionPoint();
        }
        else
        {
            position = ScanRay.GlobalTransform.origin - (ScanRay.GlobalTransform.basis.z * 10f);
        }
        node = (Node)PointerProjectRay.GetCollider();
        PlayerReticle.LerpPointAt(_Camera, position, 0.1f);

        //Update project ray
        UpdatePointerProjectRay(position);

        //Check if scan should be cleared
        if (/*!ScanRay.IsColliding() && */!PointerProjectRay.IsColliding())
        {
            //Nothing in range
            //if (FocusedInteractable != null && !WantsToClearScan)
            //{
            //    WantsToClearScan = true;
            //    ScanTimer.Start(ScanTimeout);
            //}
            WantsToClearScan = true;
            ScannedNode = null;
        }

        if (ScannedNode != node)
        {
            //Player has scanned a new node
            ScannedNode = node;
            GD.Print(ScannedNode.Name);

            //Search for Interactable
            Interactable interactable = NX.Find<Interactable>(node);
            if(interactable != null)
            {
                if(interactable != FocusedInteractable)
                {
                    //New Interactable found
                    ScanPlayer.Play();
                    PlayerReticle.PointingAtInteractable = true;

                    //Change highlight
                    interactable.Highlight = true;
                    if (FocusedInteractable != null)
                    {
                        FocusedInteractable.Highlight = false;
                    }
                }
                WantsToClearScan = false;
                ScanIsClear = false;
                FocusedInteractable = interactable;
            }
            else
            {
                ////No Interactable Found
                //if (FocusedInteractable != null && !WantsToClearScan)
                //{
                //    WantsToClearScan = true;
                //    ScanTimer.Start(ScanTimeout);
                //}
                WantsToClearScan = true;
            }
        }

        //Check if the scan should be cleared
        if (!ScanIsClear)
        {
            if (WantsToClearScan && ScanTimer.TimeLeft == 0f)
            {
                GD.Print("Start timer");
                ScanTimer.Start(ScanTimeout);
            }
            else if(!WantsToClearScan)
            {
                ScanTimer.Stop();
            }
        }

        //Get Interaction
        if (FocusedInteractable == null) return;
        if (Input.IsActionJustPressed("interact"))
        {
            FocusedInteractable.Interact();
        }
    }

    public Dictionary Save()
    {
        Dictionary data = new Dictionary
        {
            { "position" , new Array{Translation.x, Translation.y, Translation.z} },
            { "look" , new Array{CameraAngle.x, CameraAngle.y} },
            { "crouched" , Crouched },
            { "jumps" , JumpCount },
            { "health" , PlayerHealthManager.Singleton.Health }
        };
        return data;
    }

    public void Load(Dictionary data)
    {
        Array a;
        a = (Array)data["position"];
        Translation = new Vector3((float)a[0], (float)a[1], (float)a[2]);
        a = (Array)data["look"];
        CameraAngle = new Vector2((float)a[0], (float)a[1]);
        ApplyCameraAngle();
        Crouched = (bool)data["crouched"];
        JumpCount = (int)data["jumps"];
        PlayerHealthManager.Singleton.SetHealth((int)data["health"]);

        Imobile = false;
        PlayerArms.Singleton.Reset();
    }
}
