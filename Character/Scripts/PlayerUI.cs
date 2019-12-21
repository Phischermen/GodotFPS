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
    private Range Health;
    private Range Ammo;

    private Tween MyTween;

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
        MyTween = new Tween();
        AddChild(MyTween);
    }

    public static void SetHealth(int health)
    {
        Singleton.MyTween.InterpolateProperty(Singleton.Health, ":value", null, health, 1f, Tween.TransitionType.Elastic, Tween.EaseType.Out);
        Singleton.MyTween.Start();
    }

    public static void SetAmmo(int ammo)
    {
        Singleton.MyTween.InterpolateProperty(Singleton.Ammo, ":value", null, ammo, 1f, Tween.TransitionType.Elastic, Tween.EaseType.Out);
        Singleton.MyTween.Start();
    }
}
