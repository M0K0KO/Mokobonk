using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public sealed class PointVfxInstance : VfxInstance
{
    [SerializeField] private ParticleSystem _ps;
    private int _playToken;

    void Reset() { _ps = GetComponent<ParticleSystem>(); } 
    void Awake() { if (_ps == null) _ps = GetComponent<ParticleSystem>(); }

    public override void Play(in VfxPlayParams p)
    {
        transform.position = p.Origin;
        _ps.Clear(true);
        _ps.Emit(1);

        int token = ++_playToken;
        ReturnAfter(p.Lifetime, token).Forget();
    }

    private async UniTaskVoid ReturnAfter(float seconds, int token)
    {
        await UniTask.Delay(System.TimeSpan.FromSeconds(seconds), ignoreTimeScale: false);
        if (token != _playToken) return;
        ReturnToPool();
    }
}