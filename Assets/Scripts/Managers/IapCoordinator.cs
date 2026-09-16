using System;
using System.Collections.Generic;
using UnityEngine;

public class IapCoordinator : MonoBehaviour
{
    [SerializeField] private MonoBehaviour iapProvider;
    [SerializeField] private IapCatalog catalog;
    [SerializeField] private FactoryManager factory;
    [SerializeField] private bool initializeOnStart = true;

    private IIapService _iap;
    private readonly Dictionary<string, IapProductInfo> _products = new();

    public bool IsReady => _iap != null && _iap.IsReady;
    public string LastError => _iap != null ? _iap.LastError : "IAP service missing";
    public bool TryGetProduct(string id, out IapProductInfo info) => _products.TryGetValue(id, out info);

    public event Action OnReady;
    public event Action<string> OnFailed;
    public event Action<IapPurchaseInfo> OnPurchaseCompleted;

    private void Awake()
    {
        _iap = iapProvider as IIapService;
        if (_iap == null || catalog == null || factory == null)
        {
            Debug.LogError("[IapCoordinator] Missing references (IIapService / IapCatalog / FactoryManager).");
            enabled = false;
            return;
        }

        _iap.OnInitialized += HandleInitialized;
        _iap.OnInitializeFailed += HandleInitFailed;
        _iap.OnProductFetched += HandleProductFetched;
        _iap.OnPurchaseSucceeded += HandlePurchaseSucceeded;
        _iap.OnPurchaseFailed += HandlePurchaseFailed;
    }

    private void Start()
    {
        if (initializeOnStart) _iap.Initialize();
    }

    private void OnDestroy()
    {
        if (_iap == null) return;
        _iap.OnInitialized -= HandleInitialized;
        _iap.OnInitializeFailed -= HandleInitFailed;
        _iap.OnProductFetched -= HandleProductFetched;
        _iap.OnPurchaseSucceeded -= HandlePurchaseSucceeded;
        _iap.OnPurchaseFailed -= HandlePurchaseFailed;
    }

    public void Initialize() => _iap?.Initialize();

    public bool TryBuy(string productId)
    {
        if (!IsReady)
        {
            OnFailed?.Invoke("IAP is not initialized.");
            return false;
        }
        return _iap.TryPurchase(productId);
    }

    public void Restore() => _iap?.RestorePurchases();

    private void HandleInitialized() => OnReady?.Invoke();
    private void HandleInitFailed(string error) => OnFailed?.Invoke(error);

    private void HandleProductFetched(IapProductInfo info)
        => _products[info.ProductId] = info;

    private void HandlePurchaseSucceeded(IapPurchaseInfo info)
    {
        if (!catalog.TryGet(info.ProductId, out var def))
        {
            Debug.LogWarning($"[IapCoordinator] Unknown product: {info.ProductId}. Confirming to unblock store.");
            _iap.ConfirmPurchase(info.ProductId);
            return;
        }

        factory.AddCurrency(def.currencyReward);
        factory.SaveNow();
        _iap.ConfirmPurchase(info.ProductId);

        Debug.Log($"[IapCoordinator] Granted {def.currencyReward} for {info.ProductId}");
        OnPurchaseCompleted?.Invoke(info);
    }

    private void HandlePurchaseFailed(IapPurchaseFailure failure)
        => Debug.LogWarning($"[IapCoordinator] Purchase failed: {failure.ProductId} — {failure.Reason}: {failure.Message}");
}