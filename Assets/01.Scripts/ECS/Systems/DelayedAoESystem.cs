using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(MortarTargetingSystem))]
public partial struct DelayedAoESystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpatialIndexSingleton>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var idx = SystemAPI.GetSingleton<SpatialIndexSingleton>();
        var bal = SystemAPI.GetSingleton<BalanceMultiplierSingleton>();
        var healthLookup = SystemAPI.GetComponentLookup<Health>(false);
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                           .CreateCommandBuffer(state.WorldUnmanaged);
        float now = (float)SystemAPI.Time.ElapsedTime;

        state.CompleteDependency();

        foreach(var (aoe, entity) in SystemAPI.Query<RefRO<DelayedAoE>>().WithEntityAccess())
        {
            if (now < aoe.ValueRO.ExplodeTime) continue;

            int2 centerCell = SpatialIndexUtility.WorldToCell(aoe.ValueRO.Position, idx.Origin, idx.CellSize);

            int cellRadius = (int)math.ceil(aoe.ValueRO.Radius / idx.CellSize);
            float radiusSq = aoe.ValueRO.Radius * aoe.ValueRO.Radius;

            for (int dy = -cellRadius; dy <= cellRadius; dy++)
            {
                for (int dx = -cellRadius; dx <= cellRadius; dx++)
                {
                    int2 cell = centerCell + new int2(dx, dy);
                    if (!idx.Map.TryGetFirstValue(cell, out var entry, out var it)) continue;

                    do
                    {
                        float3 d = entry.Position - aoe.ValueRO.Position;
                        d.y = 0f;
                        if (math.lengthsq(d) <= radiusSq && healthLookup.HasComponent(entry.Entity))
                        {
                            var hp = healthLookup[entry.Entity];
                            hp.Current -= aoe.ValueRO.Damage * bal.TurretDamageMul;
                            healthLookup[entry.Entity] = hp;
                        }
                    }
                    while (idx.Map.TryGetNextValue(out entry, ref it));
                }
            }

            var vfxEntity = ecb.CreateEntity();
            ecb.AddComponent(vfxEntity, new ExplosionFiredEvent
            {
                Position = aoe.ValueRO.Position,
                Radius = aoe.ValueRO.Radius,
                VfxId = VfxId.MortarExplosion,
                SpawnTime = now,
            });

            ecb.DestroyEntity(entity);
        }
    }
}