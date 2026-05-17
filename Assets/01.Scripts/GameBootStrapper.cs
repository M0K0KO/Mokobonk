using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class GameBootStrapper : MonoBehaviour
{
    private EntityManager _entityManager;

    private Entity _queueEntity;
    private Entity _stateEntity;

    private NativeQueue<SpawnTurretCommand> _spawnQueue;
    private NativeHashMap<Unity.Mathematics.int2, Entity> _occupancyMap;

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

        _spawnQueue = new NativeQueue<SpawnTurretCommand>(Allocator.Persistent);
        _occupancyMap = new NativeHashMap<Unity.Mathematics.int2, Entity>(256, Allocator.Persistent);

        _entityManager.CreateSingleton(new SpawnTurretQueueSingleton { Queue = _spawnQueue });
        _entityManager.CreateSingleton(new GridOccupancySingleton { Map = _occupancyMap });
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

        if (_spawnQueue.IsCreated) _spawnQueue.Dispose();
        if (_occupancyMap.IsCreated) _occupancyMap.Dispose();
    }
}
