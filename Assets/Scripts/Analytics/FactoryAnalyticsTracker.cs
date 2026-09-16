using UnityEngine;

public class FactoryAnalyticsTracker : MonoBehaviour
{
    [SerializeField] private FactoryManager factory;
    [SerializeField] private IapCoordinator iap;
    [SerializeField] private AnalyticsService analytics;

    private void OnEnable()
    {
        if (factory != null)
        {
            factory.OnGameStarted += HandleGameStarted;
            factory.OnMachineUnlocked += HandleMachineUnlocked;
            factory.OnMachineUpgraded += HandleMachineUpgraded;
            factory.OnBoostStarted += HandleBoostStarted;
            factory.OnBoostFinished += HandleBoostFinished;
            factory.OnOfflineIncomeApplied += HandleOfflineIncomeApplied;
        }

        if (iap != null)
        {
            iap.OnReady += HandleIapReady;
            iap.OnFailed += HandleIapFailed;
            iap.OnProductFetched += HandleIapProductFetched;
            iap.OnPurchaseCompleted += HandlePurchaseSucceeded;
            iap.OnPurchaseFailed += HandlePurchaseFailed;
        }
    }

    private void OnDisable()
    {
        if (factory != null)
        {
            factory.OnGameStarted -= HandleGameStarted;
            factory.OnMachineUnlocked -= HandleMachineUnlocked;
            factory.OnMachineUpgraded -= HandleMachineUpgraded;
            factory.OnBoostStarted -= HandleBoostStarted;
            factory.OnBoostFinished -= HandleBoostFinished;
            factory.OnOfflineIncomeApplied -= HandleOfflineIncomeApplied;
        }

        if (iap != null)
        {
            iap.OnReady -= HandleIapReady;
            iap.OnFailed -= HandleIapFailed;
            iap.OnProductFetched -= HandleIapProductFetched;
            iap.OnPurchaseCompleted -= HandlePurchaseSucceeded;
            iap.OnPurchaseFailed -= HandlePurchaseFailed;
        }
    }

    private void HandleGameStarted(bool isFirstSession)
    {
        analytics.Track(AnalyticsEvents.GameStarted,
            (AnalyticsParams.IsFirstSession, isFirstSession),
            (AnalyticsParams.Balance, factory.Model.Balance),
            (AnalyticsParams.IncomePerSecond, factory.Model.TotalIncomePerSecond));
    }

    private void HandleMachineUnlocked(MachineModel m, double cost)
    {
        analytics.Track(AnalyticsEvents.MachineUnlocked,
            (AnalyticsParams.MachineId, m.Config.Id),
            (AnalyticsParams.Level, m.Level),
            (AnalyticsParams.Cost, cost),
            (AnalyticsParams.BalanceAfter, factory.Model.Balance));
    }

    private void HandleMachineUpgraded(MachineModel m, double cost)
    {
        analytics.Track(AnalyticsEvents.MachineUpgraded,
            (AnalyticsParams.MachineId, m.Config.Id),
            (AnalyticsParams.Level, m.Level),
            (AnalyticsParams.Cost, cost),
            (AnalyticsParams.BalanceAfter, factory.Model.Balance));
    }

    private void HandleBoostStarted(float duration, float multiplier)
    {
        analytics.Track(AnalyticsEvents.BoostStarted,
            (AnalyticsParams.DurationSeconds, duration),
            (AnalyticsParams.Multiplier, multiplier));
    }

    private void HandleBoostFinished()
    {
        analytics.Track(AnalyticsEvents.BoostFinished,
            (AnalyticsParams.Balance, factory.Model.Balance));
    }

    private void HandleOfflineIncomeApplied(OfflineResult r)
    {
        analytics.Track(AnalyticsEvents.OfflineIncomeApplied,
            (AnalyticsParams.ElapsedSeconds, r.TotalElapsedSeconds),
            (AnalyticsParams.Earned, r.Earned),
            (AnalyticsParams.BoostSeconds, r.BoostEffectiveTime),
            (AnalyticsParams.RegularSeconds, r.RegularEffectiveTime),
            (AnalyticsParams.BoostRemainingAfter, r.BoostTimeRemainingAfter));
    }

    private void HandleIapReady()
    {
        analytics.Track(AnalyticsEvents.IapInitialized,
            (AnalyticsParams.ProductCount, iap.ProductCount));
    }

    private void HandleIapFailed(string error)
    {
        analytics.Track(AnalyticsEvents.IapUnavailable,
            (AnalyticsParams.Reason, error ?? "unknown"));
    }

    private void HandleIapProductFetched(IapProductInfo info)
    {
        analytics.Track(AnalyticsEvents.IapProductFetched,
            (AnalyticsParams.ProductId, info.ProductId),
            (AnalyticsParams.LocalizedPrice, info.LocalizedPrice),
            (AnalyticsParams.Available, info.AvailableToPurchase));
    }

    private void HandlePurchaseSucceeded(IapPurchaseInfo info, float reward)
    {
        analytics.Track(AnalyticsEvents.PurchaseSucceeded,
            (AnalyticsParams.ProductId, info.ProductId),
            (AnalyticsParams.TransactionId, info.TransactionId),
            (AnalyticsParams.RewardAmount, reward),
            (AnalyticsParams.BalanceAfter, factory.Model.Balance));
    }

    private void HandlePurchaseFailed(IapPurchaseFailure failure)
    {
        analytics.Track(AnalyticsEvents.PurchaseFailed,
            (AnalyticsParams.ProductId, failure.ProductId),
            (AnalyticsParams.Reason, failure.Reason),
            (AnalyticsParams.Message, failure.Message));
    }
}
