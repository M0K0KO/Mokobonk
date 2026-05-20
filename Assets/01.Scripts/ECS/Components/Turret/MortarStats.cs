using Unity.Entities;

public struct MortarStats : IComponentData
{
    public float Range;
    public float FireRate;
    public float Damage;
    public float AoERadius;
    public float ExplodeDelay;
    public float Cooldown;
}