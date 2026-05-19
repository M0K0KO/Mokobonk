using System;
using UnityEngine;

public sealed class BeamVfxInstance : VfxInstance
{
    [SerializeField] private LineRenderer _lr;
    [SerializeField] private BeamFader _fader;
    private Action _cachedOnFadeComplete;
    void Reset()
    {
        _lr = GetComponent<LineRenderer>();
        _fader = GetComponent<BeamFader>();
    }
    void Awake()
    {
        if (_lr == null) _lr = GetComponent<LineRenderer>();
        if (_fader == null) _fader = GetComponent<BeamFader>();
        _cachedOnFadeComplete = OnFadeComplete;
    }
    public override void Play(in VfxPlayParams p)
    {
        _lr.SetPosition(0, p.Origin);
        _lr.SetPosition(1, p.End);
        _lr.enabled = true;
        _fader.Begin(p.Lifetime, _cachedOnFadeComplete);
    }

    private void OnFadeComplete() => ReturnToPool();
}