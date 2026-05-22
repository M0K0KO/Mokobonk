using Unity.Entities;

public struct EnemyAttackStats : IComponentData
{
    public float Damage;
    public float Interval;
    public float Range;
    public float Cooldown;
}