using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class GameBootStrapper : MonoBehaviour
{
    private EntityManager _entityManager;

    private Entity _queueEntity;
    private Entity _stateEntity;

    private NativeQueue<SpawnTurretCommand> _turretSpawnQueue;
    private NativeQueue<SpawnWallCommand> _wallSpawnQueue;

    private NativeHashMap<int2, Entity> _occupancyMap;

    private NativeArray<float3> _flowDirections;
    private NativeArray<ushort> _flowCosts;

    private void Awake()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        var damageQueue = new NativeQueue<float>(Allocator.Persistent);

        _queueEntity = _entityManager.CreateSingleton(new CoreDamageQueueSingleton
        {
            Queue = damageQueue
        });

        _stateEntity = _entityManager.CreateSingleton(new GameStateSingleton
        {
            State = GameState.Playing
        });

        _turretSpawnQueue = new NativeQueue<SpawnTurretCommand>(Allocator.Persistent);
        _wallSpawnQueue = new NativeQueue<SpawnWallCommand>(Allocator.Persistent);
        _occupancyMap = new NativeHashMap<int2, Entity>(256, Allocator.Persistent);

        _entityManager.CreateSingleton(new SpawnTurretQueueSingleton { Queue = _turretSpawnQueue });
        _entityManager.CreateSingleton(new SpawnWallQueueSingleton { Queue = _wallSpawnQueue });
        _entityManager.CreateSingleton(new GridOccupancySingleton { Map = _occupancyMap });
    }

    private void Start()
    {
        var gridQuery = _entityManager.CreateEntityQuery(typeof(GridConfigSingleton));
        if (!gridQuery.TryGetSingleton<GridConfigSingleton>(out var grid))
        {
            Debug.LogError("GridConfigSingleton not baked yet. Check SubScene loaded.");
            return;
        }

        int w = grid.GridSize.x;
        int h = grid.GridSize.y;
        int total = w * h;

        _flowDirections = new NativeArray<float3>(total, Allocator.Persistent);
        _flowCosts = new NativeArray<ushort>(total, Allocator.Persistent);

        _entityManager.CreateSingleton(new FlowFieldSingleton
        {
            Directions = _flowDirections,
            Costs = _flowCosts,
            Width = w,
            Height = h
        });

        _entityManager.CreateSingleton(new FlowFieldDirtyFlag { Value = true });
    }

    private void OnDestroy()
    {
        if (World.DefaultGameObjectInjectionWorld != null
            && _queueEntity != Entity.Null
            && _entityManager.Exists(_queueEntity))
        {
            var queueSingleton = _entityManager.GetComponentData<CoreDamageQueueSingleton>(_queueEntity);
            if (queueSingleton.Queue.IsCreated)
            {
                queueSingleton.Queue.Dispose();
            }
        }

        if (_turretSpawnQueue.IsCreated) _turretSpawnQueue.Dispose();
        if (_wallSpawnQueue.IsCreated) _wallSpawnQueue.Dispose();
        if (_occupancyMap.IsCreated) _occupancyMap.Dispose();
        if (_flowDirections.IsCreated) _flowDirections.Dispose();
        if (_flowCosts.IsCreated) _flowCosts.Dispose();
    }
}
