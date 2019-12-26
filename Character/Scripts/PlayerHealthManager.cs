using Godot;
using System;

public sealed class PlayerHealthManager : HealthManager
{
    private PlayerHealthManager():base(100, 100) { }
    private static PlayerHealthManager _singleton;
    public static PlayerHealthManager Singleton
    {
        get { return _singleton; }
        private set
        {
            SI.SetAndWarnUser(ref value, ref _singleton);
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

    public override void TakeDamage(string damageSource, int amount, Node attacker = null)
    {
        base.TakeDamage(damageSource, amount, attacker);
        PlayerUI.SetHealth(Health);
    }

    public override void SetHealth(int health)
    {
        base.SetHealth(health);
        PlayerUI.SetHealth(health);
    }

    public override void Kill(string deathSource)
    {
        base.Kill(deathSource);
        PlayerUI.SetHealth(0);
        //Play animations
        //Disable movement
    }
}
