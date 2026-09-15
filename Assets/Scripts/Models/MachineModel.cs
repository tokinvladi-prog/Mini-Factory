using System;
using UnityEngine;

public class MachineModel
{
    public MachineConfig Config { get; }
    public int Level { get; private set; }
    public bool IsUnlocked { get; private set; }

    public MachineModel(MachineConfig config)
    {
        Config = config;
        Level = 0;
        IsUnlocked = false;
    }

    public float GetIncome() => IsUnlocked ? Config.BaseIncome * Level : 0f;
    public bool CanUnlock(float balance) => !IsUnlocked && balance >= Config.BaseUnlockCost;
    public bool CanUpgrade(float balance) => IsUnlocked && balance >= GetUpgradeCost();

    public float GetUpgradeCost()
    {
        int exponent = Mathf.Max(0, Level - 1);
        return Config.BaseUpgradeCost * Mathf.Pow(Config.CostMultiplier, exponent);
    }

    public bool Unlock()
    {
        if (IsUnlocked) return false;
        IsUnlocked = true;
        Level = 1;
        return true;
    }

    public bool Upgrade()
    {
        if (!IsUnlocked) return false;
        Level++;
        return true;
    }

    public void Restore(int level, bool isUnlocked)
    {
        IsUnlocked = isUnlocked;
        Level = isUnlocked ? Mathf.Max(1, level) : 0;
    }
}
