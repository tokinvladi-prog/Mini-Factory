using NUnit.Framework;

public class FactoryModelTests
{
    [Test]
    public void TryUpgrade_DeductsBalance_AndRaisesIncome()
    {
        var cfg = TestConfigFactory.Machine(
            "press",
            baseIncome: 10f,
            baseUpgradeCost: 25f);

        var model = new FactoryModel(TestConfigFactory.Factory(machines: cfg),
                                     startingBalance: 100f);
        var machine = model.Machines[0];
        model.TryUnlock(machine);

        float costBefore = machine.GetUpgradeCost();
        float incomeBefore = model.TotalIncomePerSecond;

        Assert.IsTrue(model.TryUpgrade(machine));

        Assert.AreEqual(90f - costBefore, model.Balance, 1e-9);
        Assert.Greater(model.TotalIncomePerSecond, incomeBefore);
    }

    [Test]
    public void TryUpgrade_NotEnoughBalance_LeavesStateUntouched()
    {
        var cfg = TestConfigFactory.Machine(
            "press",
            baseUpgradeCost: 1000f);

        var model = new FactoryModel(TestConfigFactory.Factory(machines: cfg),
                                     startingBalance: 5f);
        var machine = model.Machines[0];
        machine.Restore(level: 1, isUnlocked: true);

        float balanceBefore = model.Balance;
        int levelBefore = machine.Level;

        Assert.IsFalse(model.TryUpgrade(machine),
            "Апгрейд при нехватке денег должен быть отклонён.");

        Assert.AreEqual(balanceBefore, model.Balance, 1e-9);
        Assert.AreEqual(levelBefore, machine.Level);
    }

    [Test]
    public void Tick_AccumulatesBalance_ByIncomeTimesDelta()
    {
        var cfg = TestConfigFactory.Machine("press", baseIncome: 5f);
        var model = new FactoryModel(TestConfigFactory.Factory(machines: cfg),
                                     startingBalance: 0f);
        model.Machines[0].Restore(level: 1, isUnlocked: true);

        model.Tick(10f);

        Assert.AreEqual(50d, model.Balance, 1e-9);
    }

    [Test]
    public void TryUpgrade_ForeignMachine_IsRejected()
    {
        var cfgA = TestConfigFactory.Machine("press");
        var model = new FactoryModel(TestConfigFactory.Factory(machines: cfgA),
                                     startingBalance: 1000f);

        var foreign = new MachineModel(TestConfigFactory.Machine("ghost"));
        foreign.Restore(level: 1, isUnlocked: true);

        float before = model.Balance;

        Assert.IsFalse(model.TryUpgrade(foreign));
        Assert.AreEqual(before, model.Balance, 1e-9,
            "Апгрейд чужой машины не должен списывать деньги.");
    }

    [Test]
    public void Upgrade_FiresEventsOnce()
    {
        var cfg = TestConfigFactory.Machine("press", baseUpgradeCost: 5f);
        var model = new FactoryModel(TestConfigFactory.Factory(machines: cfg),
                                     startingBalance: 100f);
        var machine = model.Machines[0];
        machine.Restore(level: 1, isUnlocked: true);

        int balanceEvents = 0, incomeEvents = 0, machineEvents = 0;
        model.OnBalanceChanged += _ => balanceEvents++;
        model.OnIncomeChanged += _ => incomeEvents++;
        model.OnMachineChanged += _ => machineEvents++;

        model.TryUpgrade(machine);

        Assert.AreEqual(1, balanceEvents);
        Assert.AreEqual(1, incomeEvents);
        Assert.AreEqual(1, machineEvents);
    }
}
