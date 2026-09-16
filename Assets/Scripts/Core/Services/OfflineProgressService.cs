using UnityEngine;

public static class OfflineProgressService
    {
        public static OfflineResult Compute(
            float totalElapsedSeconds,
            float boostTimeRemaining,
            float boostMultiplier,
            float incomePerSecond,
            float maxOfflineSeconds)
        {
            totalElapsedSeconds = Mathf.Max(0f, totalElapsedSeconds);
            boostTimeRemaining = Mathf.Max(0f, boostTimeRemaining);
            maxOfflineSeconds = Mathf.Max(0f, maxOfflineSeconds);

            if (totalElapsedSeconds <= 0f)
                return OfflineResult.Empty(boostTimeRemaining);

            float cappedElapsed = Mathf.Min(totalElapsedSeconds, maxOfflineSeconds);
            float boostEffectiveTime = Mathf.Min(cappedElapsed, boostTimeRemaining);
            float regularEffectiveTime = Mathf.Max(0f, cappedElapsed - boostEffectiveTime);

            float earned = incomePerSecond * boostMultiplier * boostEffectiveTime
                          + incomePerSecond * regularEffectiveTime;

            float boostRemainingAfter = Mathf.Max(0f, boostTimeRemaining - totalElapsedSeconds);

            return new OfflineResult(
                earned,
                boostEffectiveTime,
                regularEffectiveTime,
                boostRemainingAfter,
                totalElapsedSeconds);
        }
    }