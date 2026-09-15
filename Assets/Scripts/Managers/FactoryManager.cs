using System;
using UnityEngine;

public class FactoryManager : MonoBehaviour
{
    [SerializeField] private FactoryConfig config;
    [SerializeField] private float startingBalance = 0f;

    public FactoryModel Model { get; private set; }

    public event Action<float> OnBalanceChanged;
    public event Action<float> OnIncomeChanged;
    public event Action<MachineModel> OnMachineChanged;

    private void Awake()
    {
        Model = new FactoryModel(config, startingBalance);
        Model.OnBalanceChanged += HandleModelBalanceChanged;
        Model.OnIncomeChanged += HandleModelIncomeChanged;
        Model.OnMachineChanged += HandleModelMachineChanged;
    }

    private void Update()
    {
        if (Model == null) return;
        Model.Tick(Time.deltaTime);
    }

    private void HandleModelBalanceChanged(float v) => OnBalanceChanged?.Invoke(v);
    private void HandleModelIncomeChanged(float v) => OnIncomeChanged?.Invoke(v);
    private void HandleModelMachineChanged(MachineModel m) => OnMachineChanged?.Invoke(m);
}
