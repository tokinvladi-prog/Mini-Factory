using NUnit.Framework;

public class OfflineProgressServiceTests
{
    private const float Income = 1f;
    private const float BoostMult = 2f;
    private const float MaxOffline = 3600f;

    [Test]
    public void NoBoost_FullWindow_ReturnsIncomeTimesElapsed()
    {
        var r = OfflineProgressService.Compute(
            totalElapsedSeconds: 100f,
            boostTimeRemaining: 0f,
            boostMultiplier: BoostMult,
            incomePerSecond: Income,
            maxOfflineSeconds: MaxOffline);

        Assert.AreEqual(100f, r.Earned, 1e-9);
        Assert.AreEqual(0f, r.BoostEffectiveTime);
        Assert.AreEqual(100f, r.RegularEffectiveTime);
    }

    [Test]
    public void WithBoost_ChargesBoostAtMultiplier_ThenRegular()
    {
        var r = OfflineProgressService.Compute(
            totalElapsedSeconds: 100f,
            boostTimeRemaining: 30f,
            boostMultiplier: BoostMult,
            incomePerSecond: Income,
            maxOfflineSeconds: MaxOffline);

        Assert.AreEqual(130f, r.Earned, 1e-9);
        Assert.AreEqual(30f, r.BoostEffectiveTime);
        Assert.AreEqual(70f, r.RegularEffectiveTime);
        Assert.AreEqual(0f, r.BoostTimeRemainingAfter,
            "Буст израсходован полностью: 30 - 100 -> 0.");
    }

    [Test]
    public void ElapsedExceedsMaxOffline_IsCapped()
    {
        var r = OfflineProgressService.Compute(
            totalElapsedSeconds: 7200f,
            boostTimeRemaining: 0f,
            boostMultiplier: BoostMult,
            incomePerSecond: Income,
            maxOfflineSeconds: MaxOffline);

        Assert.AreEqual(3600f, r.Earned, 1e-9);
        Assert.AreEqual(3600f, r.RegularEffectiveTime);
    }

    [Test]
    public void LongBoost_DoesNotProduceNegativeRegularTime()
    {
        var r = OfflineProgressService.Compute(
            totalElapsedSeconds: 100f,
            boostTimeRemaining: 7200f,
            boostMultiplier: BoostMult,
            incomePerSecond: Income,
            maxOfflineSeconds: MaxOffline);

        Assert.AreEqual(200f, r.Earned, 1e-9, "100 сек * 2x = 200.");
        Assert.AreEqual(100f, r.BoostEffectiveTime);
        Assert.AreEqual(0f, r.RegularEffectiveTime,
            "Отрицательный regular — баг формулы из ТЗ, у нас защищено.");
        Assert.AreEqual(7100f, r.BoostTimeRemainingAfter, 1e-9,
            "Буст тикает по реальному времени: 7200 - 100.");
    }

    [Test]
    public void ZeroElapsed_ReturnsEmptyResult()
    {
        var r = OfflineProgressService.Compute(
            totalElapsedSeconds: 0f,
            boostTimeRemaining: 30f,
            boostMultiplier: BoostMult,
            incomePerSecond: Income,
            maxOfflineSeconds: MaxOffline);

        Assert.IsFalse(r.HasReward);
        Assert.AreEqual(0f, r.Earned, 1e-9);
        Assert.AreEqual(30f, r.BoostTimeRemainingAfter,
            "При нулевом оффлайне буст не тратится.");
    }

    [TestCase(0f, 0f, 0f)]
    [TestCase(50f, 0f, 50f)]
    [TestCase(50f, 20f, 70f)]
    [TestCase(50f, 50f, 100f)]
    [TestCase(50f, 999f, 100f)]
    public void Earned_MatchesSpecification(float elapsed, float boostRemaining, float expected)
    {
        var r = OfflineProgressService.Compute(
            totalElapsedSeconds: elapsed,
            boostTimeRemaining: boostRemaining,
            boostMultiplier: BoostMult,
            incomePerSecond: Income,
            maxOfflineSeconds: MaxOffline);

        Assert.AreEqual(expected, r.Earned, 1e-9);
    }
}