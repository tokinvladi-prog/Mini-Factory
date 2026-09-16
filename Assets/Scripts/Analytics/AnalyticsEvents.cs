public static class AnalyticsEvents
{
    public const string GameStarted = "game_started";
    public const string MachineUnlocked = "machine_unlocked";
    public const string MachineUpgraded = "machine_upgraded";
    public const string BoostStarted = "boost_started";
    public const string BoostFinished = "boost_finished";
    public const string OfflineIncomeApplied = "offline_income_applied";

    public const string IapInitialized = "iap_initialized";
    public const string IapUnavailable = "iap_unavailable";
    public const string IapProductFetched = "iap_product_fetched";
    public const string PurchaseSucceeded = "purchase_succeeded";
    public const string PurchaseFailed = "purchase_failed";
}

public static class AnalyticsParams
{
    public const string IsFirstSession = "is_first_session";
    public const string Balance = "balance";
    public const string BalanceAfter = "balance_after";
    public const string IncomePerSecond = "income_per_second";

    public const string MachineId = "machine_id";
    public const string Level = "level";
    public const string Cost = "cost";

    public const string DurationSeconds = "duration_seconds";
    public const string Multiplier = "multiplier";

    public const string ElapsedSeconds = "elapsed_seconds";
    public const string Earned = "earned";
    public const string BoostSeconds = "boost_seconds";
    public const string RegularSeconds = "regular_seconds";
    public const string BoostRemainingAfter = "boost_remaining_after";

    public const string ProductId = "product_id";
    public const string LocalizedPrice = "localized_price";
    public const string Available = "available";
    public const string ProductCount = "product_count";

    public const string TransactionId = "transaction_id";
    public const string RewardAmount = "reward_amount";
    public const string Reason = "reason";
    public const string Message = "message";
}
