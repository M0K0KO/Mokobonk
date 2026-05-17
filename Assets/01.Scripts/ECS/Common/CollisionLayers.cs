using UnityEngine;

public static class CollisionLayers
{
    public const uint Core     = 1u << 0;
    public const uint Enemy      = 1u << 1;
    public const uint Projectile = 1u << 2;
    public const uint Turret = 1u << 3;
}
