using Godot;
using System.Collections;

public class PlayerFeet : Area
{
    private PlayerFeet() { }
    private static PlayerFeet _singleton;
    public static PlayerFeet Singleton
    {
        get { return _singleton; }
        private set
        {
            SI.SetAndWarnUser(ref value, ref _singleton);
        }
    }

    public GroundSound CurrentGroundSound;
    public SortedList OverlappedGroundSounds;

    public int Bodies = 0;
    public bool CanBunnyHop = true;
    public AudioStreamPlayer3D StepPlayer { get; private set; }
    public AudioStreamPlayer3D JumpPlayer { get; private set; }
    public AudioStreamPlayer3D LandPlayer { get; private set; }
    public Timer StepTimer { get; private set; }

    private void UpdateAudioPlayers()
    {
        //Check if empty
        //GD.Print(OverlappedGroundSounds.Count);
        if(OverlappedGroundSounds.Count == 0)
        {
            return;
        }

        //Check for groundsound with highest priority
        int idx = OverlappedGroundSounds.Count - 1;
        if (OverlappedGroundSounds.GetByIndex(idx) != CurrentGroundSound)
        {
            GroundSound newGroundSound = (GroundSound)OverlappedGroundSounds.GetByIndex(idx);
            StepPlayer.Stream = newGroundSound.StepSound;
            JumpPlayer.Stream = newGroundSound.JumpSound;
            LandPlayer.Stream = newGroundSound.LandSound;
            CurrentGroundSound = newGroundSound;
        }
    }

    public override void _EnterTree()
    {
        base._EnterTree();
        Singleton = this;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        Singleton = null;
    }

    public override void _Ready()
    {
        base._Ready();
        OverlappedGroundSounds = new SortedList();
        StepPlayer = GetNode<AudioStreamPlayer3D>("Player1");
        JumpPlayer = GetNode<AudioStreamPlayer3D>("Player2");
        LandPlayer = GetNode<AudioStreamPlayer3D>("Player3");
        StepTimer = GetNode<Timer>("StepTimer");
        StepTimer.Connect("timeout", this, "_OnStepTimerTimeout");
        Connect("body_entered", this, "_OnBodyEntered");
        Connect("body_exited", this, "_OnBodyExited");
    }

    private void _OnBodyEntered(object body)
    {
        //Increment number of bodies
        Bodies += 1;

        //Search for GroundSound Node. If found, update sound library.
        GroundSound groundSound = NX.Find<GroundSound>((Node)body);
        if (groundSound != null)
        {
            if (!OverlappedGroundSounds.ContainsKey(groundSound.Priority))
            {
                OverlappedGroundSounds.Add(groundSound.Priority, groundSound);
                UpdateAudioPlayers();
            }
            else
            {
                OverlappedGroundSounds[groundSound.Priority] = groundSound;
            }
        }
        //Allow character to bunny hop again
        CanBunnyHop = true;
    }

    private void _OnBodyExited(object body)
    {
        //Decrement number of bodies
        Bodies -= 1;

        //Search for GroundSound Node. If found, update sound library.
        GroundSound groundSound = NX.Find<GroundSound>((Node)body);
        if (groundSound != null && groundSound == OverlappedGroundSounds[groundSound.Priority])
        {
            OverlappedGroundSounds.Remove(groundSound.Priority);
            UpdateAudioPlayers();
        }
    }

    private void _OnStepTimerTimeout()
    {
        //GD.Print("Step");
        StepPlayer.Play();
    }
}



