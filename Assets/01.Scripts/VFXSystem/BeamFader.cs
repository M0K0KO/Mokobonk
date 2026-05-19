using System;
using UnityEngine;

public class BeamFader : MonoBehaviour
{
    [SerializeField]  private LineRenderer _lr;
    private float _life;
    private float _t;
    private Gradient _baseGradient;
    private Action _onComplete;

    void Reset() { _lr = GetComponent<LineRenderer>(); }
    void Awake()
    {
        _lr = GetComponent<LineRenderer>();
        _baseGradient = _lr.colorGradient;
    }

    public void Begin(float lifetime, Action onComplete)
    {
        _life = lifetime;
        _t = 0f;
        _onComplete = onComplete;
    }

    void Update()
    {
        if (_life <= 0f) return;
        _t += Time.deltaTime;
        float u = Mathf.Clamp01(_t / _life);

        var grad = new Gradient();
        var ck = _baseGradient.colorKeys;
        var ak = _baseGradient.alphaKeys;
        for (int i = 0; i < ak.Length; i++)
            ak[i].alpha *= (1f - u);
        grad.SetKeys(ck, ak);
        _lr.colorGradient = grad;

        if (u >= 1f)
        {
            _lr.enabled = false;
            _life = 0f;
            _onComplete?.Invoke();
        }
    }
}