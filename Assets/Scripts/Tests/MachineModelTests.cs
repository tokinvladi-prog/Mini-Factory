using NUnit.Framework;

public class MachineModelTests
{
    [Test]
    public void NewMachine_IsLocked_AndHasZeroIncome()
    {
        var cfg = TestConfigFactory.Machine("press", baseIncome: 12.5d);
        var m = new MachineModel(cfg);

        Assert.IsFalse(m.IsUnlocked, "Новая машина должна быть заперта.");
        Assert.AreEqual(0, m.Level, "Уровень до Unlock — 0.");
        Assert.AreEqual(0d, m.GetIncome(), 1e-9, "Доход запертой машины — 0.");
    }

    [Test]
    public void Unlock_SetsLevelOne_AndEnablesIncome()
    {
        var cfg = TestConfigFactory.Machine("press", baseIncome: 12.5d);
        var m = new MachineModel(cfg);

        Assert.IsTrue(m.Unlock());
        Assert.IsTrue(m.IsUnlocked);
        Assert.AreEqual(1, m.Level);
        Assert.AreEqual(12.5d, m.GetIncome(), 1e-9);
    }

    [Test]
    public void UpgradeCost_GrowsGeometrically()
    {
        var cfg = TestConfigFactory.Machine(
            "press",
            baseUpgradeCost: 100d,
            costMultiplier: 1.15f);
        var m = new MachineModel(cfg);
        m.Unlock();

        Assert.AreEqual(100d, m.GetUpgradeCost(), 1e-9);

        m.Upgrade();
        Assert.AreEqual(115d, m.GetUpgradeCost(), 1e-6);

        m.Upgrade();
        Assert.AreEqual(132.25d, m.GetUpgradeCost(), 1e-6);
    }

    [Test]
    public void Income_ScalesLinearlyWithLevel()
    {
        var cfg = TestConfigFactory.Machine("press", baseIncome: 10d);
        var m = new MachineModel(cfg);
        m.Unlock();
        m.Upgrade();
        m.Upgrade();

        Assert.AreEqual(3, m.Level);
        Assert.AreEqual(30d, m.GetIncome(), 1e-9);
    }

    [Test]
    public void Restore_UnlockedLevelZero_IsClampedToOne()
    {
        var cfg = TestConfigFactory.Machine("press", baseIncome: 10d);
        var m = new MachineModel(cfg);

        m.Restore(level: 0, isUnlocked: true);

        Assert.IsTrue(m.IsUnlocked);
        Assert.AreEqual(1, m.Level, "Уровень разблокированной машины не может быть 0.");
    }
}
