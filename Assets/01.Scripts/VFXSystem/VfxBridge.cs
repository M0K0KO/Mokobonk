using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public sealed class VfxBridge : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private VfxRegistry _registry;
    [SerializeField] private int _maxEventsPerFrame = 50;
    [SerializeField] private float _maxEventAge = 0.1f;
    [SerializeField] private int _maxQueueSize = 500;

    private VfxPool _pool;
    private EntityManager _entityManager;
    private EntityQuery _hitScanQuery, _explosionQuery, _buildableDeathQuery;

    private readonly Queue<HitscanFiredEvent> _hitscanQueue = new(512);
    private readonly Queue<ExplosionFiredEvent> _explosionQueue = new(512);
    private readonly Queue<BuildableDeathEvent> _buildableDeathQueue = new(512);

    private void Start()
    {
        _registry.BuildLookup();

        _pool = new VfxPool(transform);
        foreach (var e in _registry.Entries)
            _pool.Register(e.Id, e.Prefab, e.PrewarmCount);

        var world = World.DefaultGameObjectInjectionWorld;
        _entityManager = world.EntityManager;
        _hitScanQuery = _entityManager.CreateEntityQuery(typeof(HitscanFiredEvent));
        _explosionQuery = _entityManager.CreateEntityQuery(typeof(ExplosionFiredEvent));
        _buildableDeathQuery = _entityManager.CreateEntityQuery(typeof(BuildableDeathEvent));
    }

    private void Update()
    {
        CollectHitscanEvents();
        CollectExplosionEvents();
        CollectBuildableDeathEvents();
        DispatchEvents();
    }

    private void CollectHitscanEvents()
    {
        if (_hitScanQuery.IsEmpty) return;
        var events = _hitScanQuery.ToComponentDataArray<HitscanFiredEvent>(Unity.Collections.Allocator.Temp);

        int spaceLeft = _maxQueueSize - _hitscanQueue.Count;
        int toCopy = Mathf.Min(events.Length, spaceLeft);
        for (int i = 0; i < toCopy; i++)
            _hitscanQueue.Enqueue(events[i]);

        _entityManager.DestroyEntity(_hitScanQuery);
        events.Dispose();
    }

    private void CollectExplosionEvents()
    {
        if (_explosionQuery.IsEmpty) return;
        var events = _explosionQuery.ToComponentDataArray<ExplosionFiredEvent>(Unity.Collections.Allocator.Temp);

        int spaceLeft = _maxQueueSize - _explosionQueue.Count;
        int toCopy = Mathf.Min(events.Length, spaceLeft);
        for (int i = 0; i < toCopy; i++)
            _explosionQueue.Enqueue(events[i]);

        _entityManager.DestroyEntity(_explosionQuery);
        events.Dispose();
    }

    private void CollectBuildableDeathEvents()
    {
        if (_buildableDeathQuery.IsEmpty) return;
        var events = _buildableDeathQuery.ToComponentDataArray<BuildableDeathEvent>(Unity.Collections.Allocator.Temp);

        int spaceLeft = _maxQueueSize - _buildableDeathQueue.Count;
        int toCopy = Mathf.Min(events.Length, spaceLeft);
        for (int i = 0; i < toCopy; i++)
            _buildableDeathQueue.Enqueue(events[i]);

        _entityManager.DestroyEntity(_buildableDeathQuery);
        events.Dispose();
    }

    private void DispatchEvents()
    {
        float now = (float)World.DefaultGameObjectInjectionWorld.Time.ElapsedTime;
        int processed = 0;
        
        while (_hitscanQueue.Count > 0 && processed < _maxEventsPerFrame)
        {
            var evt = _hitscanQueue.Dequeue();
            if (now - evt.SpawnTime > _maxEventAge) continue;
            Play(evt);
            processed++;
        }

        while (_explosionQueue.Count > 0 && processed < _maxEventsPerFrame)
        {
            var evt = _explosionQueue.Dequeue();
            if (now - evt.SpawnTime > _maxEventAge) continue;
            Play(evt);
            processed++;
        }

        while (_buildableDeathQueue.Count > 0 && processed < _maxEventsPerFrame)
        {
            var evt = _buildableDeathQueue.Dequeue();
            if (now - evt.SpawnTime > _maxEventAge) continue;
            Play(evt);
            processed++;
        }
    }

    private void Play(in HitscanFiredEvent evt)
    {
        if (evt.MuzzleId != VfxId.None)
        {
            var entry = _registry.Get(evt.MuzzleId);
            var inst = _pool.Rent(evt.MuzzleId);
            inst.Play(new VfxPlayParams
            {
                Origin = evt.Origin,
                Lifetime = entry.Lifetime,
            });
        }
        if (evt.BeamId != VfxId.None)
        {
            var entry = _registry.Get(evt.BeamId);
            var inst = _pool.Rent(evt.BeamId);
            inst.Play(new VfxPlayParams
            {
                Origin = evt.Origin,
                End = evt.Hit,
                Lifetime = entry.Lifetime,
            });
        }
        if (evt.HitSparkId != VfxId.None)
        {
            var entry = _registry.Get(evt.HitSparkId);
            var inst = _pool.Rent(evt.HitSparkId);
            inst.Play(new VfxPlayParams
            {
                Origin = evt.Hit,
                Lifetime = entry.Lifetime,
            });
        }
    }

    private void Play(in ExplosionFiredEvent evt)
    {
        if (evt.VfxId == VfxId.None) return;
        var entry = _registry.Get(evt.VfxId);
        var inst = _pool.Rent(evt.VfxId);
        inst.Play(new VfxPlayParams
        {
            Origin = evt.Position,
            Lifetime = entry.Lifetime,
        });
    }

    private void Play(in BuildableDeathEvent evt)
    {
        var entry = _registry.Get(evt.VfxId);
        var inst = _pool.Rent(evt.VfxId);
        inst.Play(new VfxPlayParams
        {
            Origin = evt.Position,
            Lifetime = entry.Lifetime,
        });
    }
}