using Godot;
using System;

public class HealthManager : Node
{
    private HealthManager() { }
    public HealthManager(int health, bool clampToMax = true, bool selfDamage = true)
    {
        Health = MaxHealth = health;
        ClampToMax = clampToMax;
        SelfDamage = selfDamage;
    }

    public HealthManager(int health, int maxHealth, bool clampToMax = true, bool selfDamage = true)
    {
        Health = health;
        MaxHealth = health;
        ClampToMax = clampToMax;
        SelfDamage = selfDamage;
    }

    public int Health { get; protected set; }
    public int MaxHealth { get; protected set; }
    protected bool ClampToMax = true;
    protected bool SelfDamage = true;

    [Signal]
    public delegate void Killed(string deathSource);

    public override void _Ready()
    {
        base._Ready();
        //Set name explicitly
        Name = "HealthManager";
    }
    public virtual void TakeDamage(string damageSource, int amount, Node attacker = null)
    {
        //Check owner
        if (SelfDamage && (attacker == Owner)) return;

        //Deal damage and potentially kill player
        Health -= amount;
        if (Health <= 0) Kill(damageSource);
    }

    public virtual void SetHealth(int health)
    {
        Health = Mathf.Min(MaxHealth, health);
    }

    public virtual void Kill(string deathSource)
    {
        Health = 0;
        EmitSignal("Killed", deathSource);
    }
}
