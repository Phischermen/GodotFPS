using Godot;

public sealed class PlayerUI : Control
{
    private PlayerUI() { }
    private static PlayerUI _singleton;
    public static PlayerUI Singleton {
        get { return _singleton; }
        private set
        {
            SI.SetAndWarnUser(ref value, ref _singleton);
        }
    }

    [Export]
    public NodePath HealthPath;
    [Export]
    public NodePath AmmoPath;
	[Export]
	public NodePath ViewportPath;

    private Range Health;
    private Range Ammo;
    private TextureRect ArmTextureRect;
    private Viewport ArmViewport;

    private Tween HealthBarTween;
    private Tween ArmFlashTween;

    private readonly float FlashThreshold = 100f;
    private bool InvertTween = false; 
    private readonly Color FlashTweenStart = Colors.White;
    private readonly Color FlashTweenEnd = Colors.Pink;

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
        Health = GetNode<Range>(HealthPath);
        Ammo = GetNode<Range>(AmmoPath);
        ArmTextureRect = GetNode<TextureRect>("Arms");
        ArmViewport = GetNode<Viewport>(ViewportPath);
        //ViewportTexture texture = new ViewportTexture();
        ViewportTexture texture = (ViewportTexture)GetNode<TextureRect>("Arms").Texture;
        texture.ViewportPath = ViewportPath;
        texture.Flags = (int)Texture.FlagsEnum.Filter;
        ArmTextureRect.Texture = texture;
        HealthBarTween = new Tween();
        ArmFlashTween = new Tween();
        ArmFlashTween.Connect("tween_completed", this, "_OnArmFlashTweenCompleted");
        AddChild(HealthBarTween);
        AddChild(ArmFlashTween);
        if(ArmViewport != null) Connect("resized", this, "_OnResized");
    }

    private void _OnResized()
    {
        ArmViewport.Size = GetSize();
    }

    private bool ShouldFlash()
    {
        return PlayerHealthManager.Singleton.Health < FlashThreshold;
    }

    private void _OnArmFlashTweenCompleted(Object @object, NodePath key)
    {
        InvertTween = !InvertTween;
        if(ShouldFlash())
        {
            StartArmFlashTween();
        }
    }
    
    private void StartArmFlashTween()
    {
        Color start = InvertTween ? FlashTweenEnd : FlashTweenStart;
        Color end = InvertTween ? FlashTweenStart : FlashTweenEnd;
        Singleton.ArmFlashTween.InterpolateProperty(Singleton.ArmTextureRect, ":self_modulate", start, end, 0.5f, Tween.TransitionType.Sine, Tween.EaseType.In);
        Singleton.ArmFlashTween.Start();
    }

    private void EndArmFlashTween()
    {
        Singleton.ArmFlashTween.InterpolateProperty(Singleton.ArmTextureRect, ":self_modulate", null, FlashTweenStart, 1f, Tween.TransitionType.Linear, Tween.EaseType.In);
        Singleton.ArmFlashTween.Start();
    }

    public static void SetHealth(int health)
    {
        Singleton.HealthBarTween.InterpolateProperty(Singleton.Health, ":value", null, health, 1f, Tween.TransitionType.Elastic, Tween.EaseType.Out);
        if (Singleton.ShouldFlash())
        {
            if (!Singleton.ArmFlashTween.IsActive()) Singleton.StartArmFlashTween();
        }
        else
        {
            Singleton.EndArmFlashTween();
        }
        Singleton.HealthBarTween.Start();
    }

    public static void SetAmmo(int ammo)
    {
        Singleton.HealthBarTween.InterpolateProperty(Singleton.Ammo, ":value", null, ammo, 1f, Tween.TransitionType.Elastic, Tween.EaseType.Out);
        Singleton.HealthBarTween.Start();
    }
}
