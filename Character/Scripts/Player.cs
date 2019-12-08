using Godot;
using Godot.Collections;

public class Player : KinematicBody
{
    public static Player Singleton;

    private int Hinput;
    private int Vinput;
    private int Linput;
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
    public float ClimbSpeed = 0.2f;

    [Export]
    public float JumpHeight = 9f;
    [Export]
    public int JumpCount = 1;
    public int JumpsLeft = 0;
    [Export]
    public bool BunnyHopping = false;
    [Export]
    public bool CanCrouchJump = true;
    public bool CrouchJumped;

    [Export]
    public float HeadDipThreshold = -9f;
    [Export]
    private bool _headBobbing;
    public bool HeadBobbing
    {
        get{return _headBobbing;}
        set
        {
            if(!value)
            {
                _AnimationTree.Set("parameters/Walk1d/blend_position", 0f);
                _AnimationTree.Set("parameters/Crouch1d/blend_position", 0f);
            }
            _headBobbing = value;
        }
    }

    [Export]
    public float MaxSlopeSlip = 30f;
    [Export]
    public float MaxSlopeNoWalk = 59f;

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
                StateMachineCrouch.Travel("Crouch");
            }
            else
            {
                StateMachineCrouch.Travel("Uncrouch");
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

    private Spatial _Head;
    private Camera _Camera;
    private Timer _ClimbTimer;
    private AnimationTree _AnimationTree;
    private AnimationNodeStateMachinePlayback StateMachineCrouch;

    private JumpArea _JumpArea;
    private SnapArea _SnapArea;

    private RayCast GroundRay;
    private RayCast[] LedgeRays;
    private Label ClimbLabel;
    private Label JumpLabel;
    private Vector3 ClimbPoint;
    private Vector3 ClimbStep;
	private float ClimbDistance;

    public override void _EnterTree()
    {
        if (Singleton == null)
        {
            Singleton = this;
        }
        else
        {
            GD.Print("Two instances of Player class instantiated: ", Name, " & ", Singleton.Name);
        }
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
        OtherCollision = NX.FindAll<CollisionShape>(this);

        StandArea = GetNode<Area>("StandArea");
        _Head = GetNode<Spatial>("Head");
        _Camera = GetNode<Camera>("Head/Wrapper1/Wrapper2/Wrapper3/Camera");
        _ClimbTimer = GetNode<Timer>("ClimbTimer");
        _AnimationTree = GetNode<AnimationTree>("AnimationTree");
        StateMachineCrouch = (AnimationNodeStateMachinePlayback)_AnimationTree.Get("parameters/StateMachineCrouch/playback");

        _JumpArea = GetNode<JumpArea>("JumpArea");
        _SnapArea = GetNode<SnapArea>("SnapArea");
        GroundRay = GetNode<RayCast>("GroundRay");
        LedgeRays = new RayCast[GetNode<Node>(("Head/LedgeRays")).GetChildCount()];
        for(int i = 0; i < LedgeRays.Length; ++i)
        {
            LedgeRays[i] = GetNode<RayCast>("Head/LedgeRays/LedgeRay" + (i + 1));
        }
        ClimbLabel = GetNode<Label>("UI/ClimbLabel");
        JumpLabel = GetNode<Label>("UI/JumpLabel");

        //Check for vital actions in InputMap
        IX.CheckAndAddAction("move_forward", KeyList.W);
        IX.CheckAndAddAction("move_backward", KeyList.S);
        IX.CheckAndAddAction("move_left", KeyList.A);
        IX.CheckAndAddAction("move_right", KeyList.D);
        IX.CheckAndAddAction("move_up", KeyList.E);
        IX.CheckAndAddAction("move_down", KeyList.Q);
        IX.CheckAndAddAction("toggle_fly", KeyList.V);
        IX.CheckAndAddAction("release_mouse", KeyList.F1, false, false, true);
        IX.CheckAndAddAction("place_debug_camera", KeyList.F2, false, false, true);

        MaxSlopeSlip = Mathf.Deg2Rad(MaxSlopeSlip);
        MaxSlopeNoWalk = Mathf.Deg2Rad(MaxSlopeNoWalk);

        //Set jumps left
        JumpsLeft = JumpCount;

        //Set animation
        _AnimationTree.Active = true;
        HeadBobbing = _headBobbing;
        
        //Capture mouse
        Input.SetMouseMode(Input.MouseMode.Captured);

        StandArea.Monitoring = true;
    }

    public override void _PhysicsProcess(float delta)
    {
		GroundRay.Call("update", 0.1f + (-Velocity.y * delta));
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
        }
        if (Input.IsActionJustPressed("toggle_fly"))
        {
            Flying = !Flying;
            Velocity = Vector3.Zero;
            _AnimationTree.Active = !Flying;
        }
		
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey)
        {
            //Downcast to key
            InputEventKey eventKey = (InputEventKey)@event;

            //Update Motion Input
            if (eventKey.IsAction("move_forward")) SetMotionParameterAndConsumeInput(ref Vinput, false, eventKey, 0);
            else if (eventKey.IsAction("move_backward")) SetMotionParameterAndConsumeInput(ref Vinput, true, eventKey, 1);
            else if (eventKey.IsAction("move_left")) SetMotionParameterAndConsumeInput(ref Hinput, false, eventKey, 2);
            else if (eventKey.IsAction("move_right")) SetMotionParameterAndConsumeInput(ref Hinput, true, eventKey, 3);
            else if (eventKey.IsAction("move_up")) SetMotionParameterAndConsumeInput(ref Linput, true, eventKey, 4);
            else if (eventKey.IsAction("move_down")) SetMotionParameterAndConsumeInput(ref Linput, false, eventKey, 5);
            else if (eventKey.IsAction("release_mouse") && eventKey.Pressed)
            {
                Input.SetMouseMode(Input.GetMouseMode() == Input.MouseMode.Captured ? Input.MouseMode.Visible : Input.MouseMode.Captured);
            }
        }
        else if (@event is InputEventMouseMotion && Input.GetMouseMode() == Input.MouseMode.Captured)
        {
            //Downcast to mouse motion
            InputEventMouseMotion eventMouseMotion = (InputEventMouseMotion)@event;
            
            //Update camera angle
            CameraAngle.x = CameraAngle.x + eventMouseMotion.Relative.x * SensitivityX;
            CameraAngle.y = Mathf.Clamp(CameraAngle.y + eventMouseMotion.Relative.y * SensitivityY, PitchMin, PitchMax);
            
            //Apply rotation
            _Head.RotationDegrees = new Vector3(0, (InvertX ? 1 : -1) * CameraAngle.x, 0);
            _Camera.RotationDegrees = new Vector3((InvertY ? 1 : -1) * CameraAngle.y, 0, 0);
            
            //Tell event was handled
            GetTree().SetInputAsHandled();
        }
    }

    private void SetMotionParameterAndConsumeInput(ref int parameter, bool positive, InputEventKey inputEvent, int doubleCheckIndex)
    {
        GetTree().SetInputAsHandled();
        if (inputEvent.Echo) return;

        //Ensure player does not get stuck walking in one direction
        if (inputEvent.Pressed == InputDoubleChecker[doubleCheckIndex]) return;
        InputDoubleChecker[doubleCheckIndex] = inputEvent.Pressed;
        parameter += ((inputEvent.Pressed == positive) ? 1 : -1);

        //Tell Kevin he sucks and can't write working code
        if (parameter > 1 || parameter < -1)
            GD.Print("KEVIN YOUR INPUT IS STILL BROKEN!");
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
        Basis aim = _Head.GlobalTransform.basis;

        //Get slope of floor and if ground hit, set head velocity
        Vector3 normal;
        bool jumpAreaHasBodies = _JumpArea.Bodies > 0;
        if (jumpAreaHasBodies)
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
                //Reset number of jumps
                JumpsLeft = JumpCount;
                JumpLabel.Text = "Jumps Left: " + JumpCount;

                //Play land animations
				PlayerArms.Singleton.Fall = false;
				PlayerArms.Singleton.Land = true;
                if (!slipping && ImpactVelocity <= HeadDipThreshold)
                {
                	_AnimationTree.Set("parameters/Land/blend_amount", Mathf.Min(1f, Mathf.Abs((ImpactVelocity - HeadDipThreshold) / (Gravity - HeadDipThreshold))));
					_AnimationTree.Set("parameters/OneShotLand/active", true);
                    WasInAir = false;
                }

                //Determine if player had crouch jumped
                if (CrouchJumped)
                {
                    CrouchJumped = false;
                    WantsToUncrouch = crouchPressed;
                }
				
				
            }
        }
        else
        {
            WasInAir = true;
			PlayerArms.Singleton.Fall = true;
            ImpactVelocity = Velocity.y;
        }
        
        //Get input and set direction
        bool userMoving = (Vinput != 0 || Hinput != 0);
        Direction += aim.z * (slipping ? -1f : 1f) * Vinput;
        Direction += aim.x * (slipping ? -1f : 1f) * Hinput;
        Direction = Direction.Normalized();
        
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
            bob1D = Mathf.Min(1f, bob1D + (Sprint ? 0.01f : 0.04f));
            _AnimationTree.Set("parameters/StateMachine/WalkBlend/blend_position", (Sprint) ? 1.2f : 1f);
        }
        else
        {
            bob1D = Mathf.Max(0.01f, bob1D - (IsOnFloor() ? 0.075f : 0.4f));
        }
		if(HeadBobbing){
	        _AnimationTree.Set("parameters/HeadBob/blend_position", bob1D);
        }
		PlayerArms.Singleton.Walk = bob1D;

        //Get jump
		PlayerArms.Singleton.Jump = false;
        if ((BunnyHopping ? Input.IsActionPressed("jump") : Input.IsActionJustPressed("jump")) && (jumpAreaHasBodies) || Input.IsActionJustPressed("jump") && JumpsLeft > 0)
        {
			PlayerArms.Singleton.Jump = true;
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
            _JumpArea.CanBunnyHop = false;
            _SnapArea.Snap = false;

            if (!jumpAreaHasBodies)
            {
                //Update number of jumps and jump label
                JumpsLeft -= 1;
                JumpLabel.Text = "Jumps Left: " + JumpsLeft;
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
                StateMachineCrouch.Start("CrouchJump");
                Translate(Vector3.Up * 1.1f);
                //GD.Print("Crouch jump.");
            }
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
                    //GD.Print("Disabling collision ", OtherCollision[i]);
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
            if (canClimb && (bool)_ClimbTimer.Get("wants_to_climb"))
            {
                if (withCrouch)
                {
                    Crouched = true;
                }

                //Set climb point and enter climb state
                ClimbPoint = closestLedgePoint;
                ClimbStep = (ClimbPoint - Translation).Normalized() * ClimbSpeed;
                ClimbDistance = (ClimbPoint - Translation).Length();
                _AnimationTree.Set("parameters/OneShotClimb/active", true);
                _AnimationTree.Set("parameters/TimeScaleHeadBob/scale", 0f);
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

    private void Climb(float delta){
        Translation = Translation + ClimbStep;
        ClimbDistance -= ClimbSpeed;
        if(ClimbDistance <= 0)
        {
            Translation = ClimbPoint;
            Climbing = false;
            _ClimbTimer.Set("wants_to_climb",false);
            _AnimationTree.Set("parameters/TimeScaleHeadBob/scale", 1f);
        }
    }
}
