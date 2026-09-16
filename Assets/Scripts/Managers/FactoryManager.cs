using System;
using UnityEngine;

public class FactoryManager : MonoBehaviour
{
    [SerializeField] private FactoryConfig config;
    [SerializeField] private float startingBalance = 0f;

    [SerializeField, Range(5f, 60f)] private float autoSaveInterval = 12f;
    [SerializeField, Range(0f, 300f)] private float minOfflineSecondsForPopup = 30f;

    private readonly SaveService _saveService = new();
    private readonly BoostController _boost = new();

    private float _autoSaveTimer;
    private bool _dirty;
    private float _pendingOfflineReward;

    public bool HasPendingOfflineReward => _pendingOfflineReward > 0d;
    public float PendingOfflineReward => _pendingOfflineReward;

    public FactoryModel Model { get; private set; }
    public BoostController Boost => _boost;

    public event Action<float> OnBalanceChanged;
    public event Action<float> OnIncomeChanged;
    public event Action<MachineModel> OnMachineChanged;
    public event Action<float> OnOfflineRewardReady;

    private void Awake()
    {
        Model = new FactoryModel(config, startingBalance);
        LoadAndApplyOffline();

        Model.OnBalanceChanged += HandleModelBalanceChanged;
        Model.OnIncomeChanged += HandleModelIncomeChanged;
        Model.OnMachineChanged += HandleModelMachineChanged;

        if (_boost.IsActive)
            Model.IncomeMultiplier = config.BoostMultiplier;
    }

    private void OnDestroy()
    {
        if (Model == null) return;
        Model.OnBalanceChanged -= HandleModelBalanceChanged;
        Model.OnIncomeChanged -= HandleModelIncomeChanged;
        Model.OnMachineChanged -= HandleModelMachineChanged;
    }

    private void Update()
    {
        Model?.Tick(Time.deltaTime);
        TickBoost(Time.deltaTime);

        _autoSaveTimer += Time.unscaledDeltaTime;
        if (_autoSaveTimer >= autoSaveInterval)
        {
            _autoSaveTimer = 0f;
            if (_dirty) SaveNow();
        }
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause) SaveNow();
    }

    private void OnApplicationQuit() => SaveNow();

    public bool TryActivateBoost()
    {
        if (_boost.IsActive) return false;

        _boost.Start(config.BoostDuration);
        Model.IncomeMultiplier = config.BoostMultiplier;
        _dirty = true;
        return true;
    }

    public void CollectOfflineReward()
    {
        if (_pendingOfflineReward <= 0f) return;
        Model.ApplyOfflineReward(_pendingOfflineReward);
        _pendingOfflineReward = 0f;
        _dirty = true;
    }

    public void SaveNow()
    {
        if (Model == null) return;

        var dto = SaveMapper.ToDto(Model, _boost.TimeRemaining);
        _saveService.Save(dto);
        _dirty = false;
    }

    [ContextMenu("Delete Save (debug)")]
    private void DeleteSave()
    {
        _saveService.Clear();
        Debug.Log("[FactoryManager] Save deleted.");
    }

    private void TickBoost(float dt)
    {
        if (!_boost.IsActive) return;

        _boost.Tick(dt);
        if (!_boost.IsActive)
            Model.IncomeMultiplier = 1f;
        _dirty = true;
    }

    private void LoadAndApplyOffline()
    {
        if (!_saveService.HasSave()) return;

        var data = _saveService.Load();
        SaveMapper.Apply(data, Model);
        _boost.Restore(data.boostTimeRemaining);

        float elapsed = ComputeElapsedSeconds(data.lastSaveTime);
        var result = OfflineProgressService.Compute(
            elapsed,
            data.boostTimeRemaining,
            config.BoostMultiplier,
            Model.BaseIncomePerSecond,
            config.MaxOfflineSeconds);

        _boost.Restore(result.BoostTimeRemainingAfter);

        if (result.HasReward && elapsed >= minOfflineSecondsForPopup)
        {
            _pendingOfflineReward = result.Earned;
            OnOfflineRewardReady?.Invoke(result.Earned);
        }

        SaveNow();

        Debug.Log($"[Offline] elapsed={elapsed:F1}s " +
                  $"boost={result.BoostEffectiveTime:F1}s " +
                  $"regular={result.RegularEffectiveTime:F1}s " +
                  $"earned={result.Earned:F2} " +
                  $"boostLeft={result.BoostTimeRemainingAfter:F1}s");
    }

    private static float ComputeElapsedSeconds(long lastSaveUnixMs)
    {
        var last = DateTimeOffset.FromUnixTimeMilliseconds(lastSaveUnixMs);
        float elapsed = (float)(DateTimeOffset.UtcNow - last).TotalSeconds;
        return elapsed > 0f ? elapsed : 0f;
    }

    private void HandleModelBalanceChanged(float v) => OnBalanceChanged?.Invoke(v);
    private void HandleModelIncomeChanged(float v) => OnIncomeChanged?.Invoke(v);
    private void HandleModelMachineChanged(MachineModel m) => OnMachineChanged?.Invoke(m);
}
