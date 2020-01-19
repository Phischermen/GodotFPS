using Godot;
using System;

public sealed class PlayerReticle : Control
{
    private PlayerReticle() { }
    private static PlayerReticle _singleton;
    public static PlayerReticle Singleton
    {
        get { return _singleton; }
        private set
        {
            SI.SetAndWarnUser(ref value, ref _singleton);
        }
    }

    public enum DamageOrigin
    {
        Left,
        Right,
        FrontOrAbove,
        BelowOrBehind,
    }

    private bool _pointingAtInteractable;
    public static bool PointingAtInteractable
    {
        get { return Singleton._pointingAtInteractable; }
        set
        {
            if (PointingAtInteractable == value)
            {
                if (value)
                {
                    //Player has already scanned an interactable
                    Singleton.PointerTween.InterpolateProperty(Singleton.Pointer, ":rect_rotation", 0f, 90f, 0.1f, Tween.TransitionType.Linear, Tween.EaseType.Out);
                }
            }
            else
            {
                if (value)
                {
                    //Player scanned interactable
                    Singleton.PointerTween.InterpolateProperty(Singleton.Pointer, ":rect_min_size", null, GrowSize, 0.1f, Tween.TransitionType.Linear, Tween.EaseType.Out);
                    Singleton.PointerTween.InterpolateProperty(Singleton.Pointer, ":modulate", null, Colors.Yellow, 0.1f, Tween.TransitionType.Linear, Tween.EaseType.Out);
                }
                else
                {
                    //Player scanned non-interactable
                    Singleton.PointerTween.InterpolateProperty(Singleton.Pointer, ":rect_min_size", null, RestSize, 0.1f, Tween.TransitionType.Linear, Tween.EaseType.Out);
                    Singleton.PointerTween.InterpolateProperty(Singleton.Pointer, ":modulate", null, Colors.White, 0.1f, Tween.TransitionType.Linear, Tween.EaseType.Out);
                }
            }
            Singleton.PointerTween.Start();
            Singleton._pointingAtInteractable = value;
        }
    }

    Control Pointer;
    TextureRect[] Damage;

    Tween PointerTween;

    private static readonly Vector2 RestSize = new Vector2(76f, 76f);
    private static readonly Vector2 GrowSize = new Vector2(100f, 100f);

    private static float RotationTarget;

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
        Pointer = GetNode<Control>("CenterContainer/Pointer");
        Pointer.Connect("minimum_size_changed", this, "_OnPointerMinimumSizeChanged"); //TODO Define _OnPointerResized
        Damage = new TextureRect[4];

        //NOTE: Make sure Damage 1-4 is consistent with DamageOrigin
        for(int i = 0; i < 4; ++i)
        {
            Damage[i] = GetNode<TextureRect>("Damage" + (i + 1).ToString());
        }
        PointerTween = new Tween();
        AddChild(PointerTween);
    }

    private void _OnPointerMinimumSizeChanged()
    {
        float x = Pointer.RectMinSize.x / 2f;
        float y = Pointer.RectMinSize.y / 2f;
        Pointer.RectPivotOffset = new Vector2(x, y);
    }

    public static Vector2 GetPointerPosition()
    {
        return Singleton.RectPosition + Singleton.RectPivotOffset;
    }

    public static void PointAt(Camera camera, Vector3 position)
    {
        Singleton.RectPosition = camera.UnprojectPosition(position) - Singleton.RectPivotOffset;
    }

    public static void LerpPointAt(Camera camera, Vector3 position, float t)
    {
        Singleton.RectPosition = Singleton.RectPosition.LinearInterpolate(camera.UnprojectPosition(position) - Singleton.RectPivotOffset, t);
    }

}
