using UnityEngine;
public struct VfxPlayParams
{
    public Vector3 Origin;
    public Vector3 End;       // Only used in Beam(Line Renderer)
    public float Lifetime;
}
public abstract class VfxInstance : MonoBehaviour
{
    private VfxPool _pool;
    private VfxId _id;

    public void BindPool(VfxPool pool, VfxId id)
    {
        _pool = pool;
        _id = id;
    }

    public abstract void Play(in VfxPlayParams p);

    protected void ReturnToPool()
    {
        _pool.Return(_id, this);
    }
}