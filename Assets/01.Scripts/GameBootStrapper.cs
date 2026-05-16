using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class GameBootStrapper : MonoBehaviour
{
    private EntityManager _entityManager;

    private NativeQueue<float> _damageQueue;
    private Entity _queueEntity;
    private Entity _stateEntity;

    private void Awake()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        _damageQueue = new NativeQueue<float>(Allocator.Persistent);

        _queueEntity = _entityManager.CreateSingleton(new CoreDamageQueueSingleton
        {
            Queue = _damageQueue
        });

        _stateEntity = _entityManager.CreateSingleton(new GameStateSingleton
        {
            State = GameState.Playing
        });
    }

    private void OnDestroy()
    {
        if (_damageQueue.IsCreated) _damageQueue.Dispose();
    }

}
