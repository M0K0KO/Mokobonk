using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public sealed class VfxPool
{
    private readonly Dictionary<VfxId, IObjectPool<VfxInstance>> _pools = new();
    private readonly Dictionary<VfxId, GameObject> _prefabs = new();
    private readonly Transform _root;

    public VfxPool(Transform root) { _root = root; }

    public void Register(VfxId id, GameObject prefab, int prewarm, int maxSize = 200)
    {
        _prefabs[id] = prefab;

        var pool = new ObjectPool<VfxInstance>(
                createFunc: () => Create(id),
                actionOnGet: inst => inst.gameObject.SetActive(true),
                actionOnRelease: inst => inst.gameObject.SetActive(false),
                actionOnDestroy: inst => Object.Destroy(inst.gameObject),
                collectionCheck: true,    // will be automatically disabled on release build
                defaultCapacity: prewarm,
                maxSize: maxSize
            );

        _pools[id] = pool;

        var temp = new VfxInstance[prewarm];
        for (int i = 0; i < prewarm; i++) temp[i] = pool.Get();
        for (int i = 0; i < prewarm; i++) pool.Release(temp[i]);
    }

    private VfxInstance Create(VfxId id)
    {
        var go = Object.Instantiate(_prefabs[id], _root);
        go.SetActive(false);
        var inst = go.GetComponent<VfxInstance>();
        inst.BindPool(this, id);
        return inst;
    }

    public VfxInstance Rent(VfxId id) => _pools[id].Get();
    public void Return(VfxId id, VfxInstance inst) => _pools[id].Release(inst);
}