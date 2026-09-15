using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class FactoryModel
{
    private const float _balanceNotifyEpsilon = 0.01f;

    private readonly List<MachineModel> _machines = new();
    private float _lastNotifiedBalance;

    public FactoryConfig Config { get; }
    public float Balance { get; private set; }
    public IReadOnlyList<MachineModel> Machines => _machines;

    public event Action<float> OnBalanceChanged;
    public event Action<float> OnIncomeChanged;
    public event Action<MachineModel> OnMachineChanged;

    public FactoryModel(FactoryConfig config, float startingBalance = 0f)
    {
        Config = config;
        Balance = startingBalance;
        _lastNotifiedBalance = startingBalance;

        foreach (var m in config.Machines)
            _machines.Add(new MachineModel(m));
    }

    public float TotalIncomePerSecond => _machines.Sum(m => m.GetIncome());

    public void Tick(float deltaTime)
    {
        if (deltaTime <= 0d) return;
        Balance += TotalIncomePerSecond * deltaTime;
        NotifyBalanceIfSignificant();
    }

    public bool TryUnlock(MachineModel machine)
    {
        if (machine == null || !_machines.Contains(machine)) return false;
        if (!machine.CanUnlock(Balance)) return false;

        Balance -= machine.Config.BaseUnlockCost;
        machine.Unlock();
        EmitAfterMutation(machine);
        return true;
    }

    public bool TryUpgrade(MachineModel machine)
    {
        if (machine == null || !_machines.Contains(machine)) return false;
        if (!machine.CanUpgrade(Balance)) return false;

        Balance -= machine.GetUpgradeCost();
        machine.Upgrade();
        EmitAfterMutation(machine);
        return true;
    }

    public void RestoreBalance(float balance)
    {
        Balance = balance;
        _lastNotifiedBalance = balance;
        OnBalanceChanged?.Invoke(Balance);
        OnIncomeChanged?.Invoke(TotalIncomePerSecond);
    }

    private void EmitAfterMutation(MachineModel machine)
    {
        _lastNotifiedBalance = Balance;
        OnMachineChanged?.Invoke(machine);
        OnIncomeChanged?.Invoke(TotalIncomePerSecond);
        OnBalanceChanged?.Invoke(Balance);
    }

    private void NotifyBalanceIfSignificant()
    {
        if (Mathf.Abs(Balance - _lastNotifiedBalance) < _balanceNotifyEpsilon) return;
        _lastNotifiedBalance = Balance;
        OnBalanceChanged?.Invoke(Balance);
    }
}
