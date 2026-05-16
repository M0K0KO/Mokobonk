using Unity.Entities;

public struct WeaponState : IComponentData
{
    public float cooldownRemaining;
    public float fireRate;
    public float projectileSpeed;
    public float projectileDamage;
    public float projectileLifetime;
    public Entity projectilePrefab;
}