using System.Collections.Generic;
using System.Linq;

public class FactoryModel
{
    private readonly List<MachineModel> _machines = new();

    public FactoryConfig Config { get; }
    public float Balance { get; private set; }
    public IReadOnlyList<MachineModel> Machines => _machines;

    public FactoryModel(FactoryConfig config, float startingBalance = 0f)
    {
        Config = config;
        Balance = startingBalance;

        foreach (var m in config.Machines)
            _machines.Add(new MachineModel(m));
    }

    public float TotalIncomePerSecond => _machines.Sum(m => m.GetIncome());

    public void Tick(float deltaTime)
    {
        if (deltaTime <= 0d) return;
        Balance += TotalIncomePerSecond * deltaTime;
    }

    public bool TryUnlock(MachineModel machine)
    {
        if (machine == null || !_machines.Contains(machine)) return false;
        if (!machine.CanUnlock(Balance)) return false;

        Balance -= machine.Config.BaseUnlockCost;
        machine.Unlock();
        return true;
    }

    public bool TryUpgrade(MachineModel machine)
    {
        if (machine == null || !_machines.Contains(machine)) return false;
        if (!machine.CanUpgrade(Balance)) return false;

        Balance -= machine.GetUpgradeCost();
        machine.Upgrade();
        return true;
    }
}
