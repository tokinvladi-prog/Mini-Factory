using System;

public interface IIapService
{
    bool IsReady { get; }
    string LastError { get; }

    event Action OnInitialized;
    event Action<string> OnInitializeFailed;
    event Action<IapProductInfo> OnProductFetched;
    event Action<IapPurchaseInfo> OnPurchaseSucceeded;
    event Action<IapPurchaseFailure> OnPurchaseFailed;

    void Initialize();
    bool TryPurchase(string productId);
    void ConfirmPurchase(string productId);
    void RestorePurchases();
}
