using System;
using UnityEngine;

public class BoostController
{
    private float _timeRemaining;
    private int _lastNotifiedSecond = -1;

    public float TimeRemaining => _timeRemaining;
    public bool IsActive => _timeRemaining > 0f;

    public event Action<float> OnTimeChanged;
    public event Action OnExpired;

    public void Start(float duration)
    {
        _timeRemaining = duration;
        _lastNotifiedSecond = Mathf.CeilToInt(_timeRemaining);
        OnTimeChanged?.Invoke(_timeRemaining);
    }

    public void Restore(float timeRemaining)
    {
        _timeRemaining = Mathf.Max(0f, timeRemaining);
        _lastNotifiedSecond = Mathf.CeilToInt(_timeRemaining);
        OnTimeChanged?.Invoke(_timeRemaining);
    }

    public void Tick(float deltaTime)
    {
        if (_timeRemaining <= 0f || deltaTime <= 0f) return;

        _timeRemaining -= deltaTime;

        if (_timeRemaining <= 0d)
        {
            _timeRemaining = 0f;
            _lastNotifiedSecond = 0;
            OnTimeChanged?.Invoke(0f);
            OnExpired?.Invoke();
            return;
        }

        int currentSec = Mathf.CeilToInt(_timeRemaining);
        if (currentSec != _lastNotifiedSecond)
        {
            _lastNotifiedSecond = currentSec;
            OnTimeChanged?.Invoke(_timeRemaining);
        }
    }
}
