using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
partial struct WeaponFireSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<WeaponState>();
        state.RequireForUpdate<PlayerPositionSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var dt = SystemAPI.Time.DeltaTime;
        var playerPos = SystemAPI.GetSingleton<PlayerPositionSingleton>().Position;
        var weaponRW = SystemAPI.GetSingletonRW<WeaponState>();

        weaponRW.ValueRW.cooldownRemaining -= dt;
        if (weaponRW.ValueRW.cooldownRemaining > 0f) return;

        var nearestDistSq = float.MaxValue;
        var nearestPos = float3.zero;
        var found = false;

        foreach(var transform in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<EnemyTag>())
        {
            var diff = transform.ValueRO.Position - playerPos;
            var distSq = math.lengthsq(diff);
            if (distSq < nearestDistSq)
            {
                nearestDistSq = distSq;
                nearestPos = transform.ValueRO.Position;
                found = true;
            }
        }

        if (!found) return;

        var dir = math.normalizesafe(nearestPos - playerPos);

        var ecb = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        var weapon = weaponRW.ValueRO;
        var newProj = ecb.Instantiate(weapon.projectilePrefab);

        ecb.SetComponent(newProj, LocalTransform.FromPosition(playerPos));
        ecb.SetComponent(newProj, new ProjectileDirection { Value = dir });
        ecb.SetComponent(newProj, new Lifetime { Remaining = weapon.projectileLifetime });

        weaponRW.ValueRW.cooldownRemaining = weapon.fireRate;
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
