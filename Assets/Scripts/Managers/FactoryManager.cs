using System;
using UnityEngine;

public class FactoryManager : MonoBehaviour
{
    [SerializeField] private FactoryConfig config;
    [SerializeField] private float startingBalance = 0f;

    [SerializeField, Range(5f, 60f)] private float autoSaveInterval = 12f;

    private readonly SaveService _saveService = new();

    private float _autoSaveTimer;
    private bool _dirty;

    public FactoryModel Model { get; private set; }
    public float BoostTimeRemaining { get; set; }

    public event Action<float> OnBalanceChanged;
    public event Action<float> OnIncomeChanged;
    public event Action<MachineModel> OnMachineChanged;

    private void Awake()
    {
        Model = new FactoryModel(config, startingBalance);
        LoadIfExists();

        Model.OnBalanceChanged += HandleModelBalanceChanged;
        Model.OnIncomeChanged += HandleModelIncomeChanged;
        Model.OnMachineChanged += HandleModelMachineChanged;
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

    public void SaveNow()
    {
        if (Model == null) return;

        var dto = SaveMapper.ToDto(Model, BoostTimeRemaining);
        _saveService.Save(dto);
        _dirty = false;
    }

    [ContextMenu("Delete Save (debug)")]
    private void DeleteSave()
    {
        _saveService.Clear();
        Debug.Log("[FactoryManager] Save deleted.");
    }

    private void LoadIfExists()
    {
        if (!_saveService.HasSave()) return;

        var data = _saveService.Load();

        SaveMapper.Apply(data, Model);
        BoostTimeRemaining = data.boostTimeRemaining;
        _dirty = false;

        Debug.Log($"[FactoryManager] Loaded. Balance={data.balance:F2}, " +
                  $"savedAt={DateTimeOffset.FromUnixTimeMilliseconds(data.lastSaveTime):u}");
    }

    private void HandleModelBalanceChanged(float v) => OnBalanceChanged?.Invoke(v);
    private void HandleModelIncomeChanged(float v) => OnIncomeChanged?.Invoke(v);
    private void HandleModelMachineChanged(MachineModel m) => OnMachineChanged?.Invoke(m);
}
