public class OfflineResult
{
    public readonly float Earned;
    public readonly float BoostEffectiveTime;
    public readonly float RegularEffectiveTime;
    public readonly float BoostTimeRemainingAfter;
    public readonly float TotalElapsedSeconds;

    public OfflineResult(
        float earned,
        float boostEffectiveTime,
        float regularEffectiveTime,
        float boostTimeRemainingAfter,
        float totalElapsedSeconds)
    {
        Earned = earned;
        BoostEffectiveTime = boostEffectiveTime;
        RegularEffectiveTime = regularEffectiveTime;
        BoostTimeRemainingAfter = boostTimeRemainingAfter;
        TotalElapsedSeconds = totalElapsedSeconds;
    }

    public bool HasReward => Earned > 0d;

    public static OfflineResult Empty(float boostRemaining) =>
        new(0f, 0f, 0f, boostRemaining, 0f);
}
