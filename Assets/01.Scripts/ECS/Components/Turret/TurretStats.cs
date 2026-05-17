using Unity.Entities;

public struct TurretStats : IComponentData
{
    public float Range;
    public float FireRate;
    public float Cooldown;
    public float Damage;
    public float ProjectileSpeed;
    public Entity ProjectilePrefab;
}

