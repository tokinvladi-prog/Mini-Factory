using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class UnityIapService : MonoBehaviour, IIapService
{
    [SerializeField] private IapCatalog catalog;

    private StoreController _store;

    private readonly Dictionary<string, Product> _products = new();
    private readonly Dictionary<string, PendingOrder> _pendingOrders = new();

    public bool IsReady { get; private set; }
    public string LastError { get; private set; }

    public event Action OnInitialized;
    public event Action<string> OnInitializeFailed;
    public event Action<IapProductInfo> OnProductFetched;
    public event Action<IapPurchaseInfo> OnPurchaseSucceeded;
    public event Action<IapPurchaseFailure> OnPurchaseFailed;

    private void Awake() => DontDestroyOnLoad(gameObject);


    public void Initialize()
    {
        if (IsReady || _store != null) return;

        Debug.Log("[IAP] Connecting to store…");

        _store = UnityIAPServices.StoreController();

        _store.OnStoreConnected += HandleStoreConnected;
        _store.OnStoreDisconnected += HandleStoreDisconnected;
        _store.OnProductsFetched += HandleProductsFetched;
        _store.OnProductsFetchFailed += HandleProductsFetchFailed;
        _store.OnPurchasePending += HandlePurchasePending;
        _store.OnPurchaseConfirmed += HandlePurchaseConfirmed;
        _store.OnPurchaseFailed += HandlePurchaseFailed;

        _store.Connect();
    }

    public bool TryPurchase(string productId)
    {
        if (!IsReady)
        {
            EmitFailure(productId, "NotInitialized", "Store is not connected.");
            return false;
        }
        if (!_products.TryGetValue(productId, out var product))
        {
            EmitFailure(productId, "NotFound", "Product not fetched from store.");
            return false;
        }
        if (!product.availableToPurchase)
        {
            EmitFailure(productId, "NotAvailable", "Product is not available for purchase.");
            return false;
        }

        Debug.Log($"[IAP] Purchase started: {productId}");
        _store.PurchaseProduct(product);
        return true;
    }

    public void ConfirmPurchase(string productId)
    {
        if (_store == null) return;
        if (!_pendingOrders.TryGetValue(productId, out var order)) return;

        Debug.Log($"[IAP] Confirm: {productId}");
        _store.ConfirmPurchase(order);
        _pendingOrders.Remove(productId);
    }

    public void RestorePurchases()
    {
        if (_store == null) return;
        Debug.Log("[IAP] Restore requested.");
        _store.RestoreTransactions((success, error) =>
            Debug.Log($"[IAP] Restore finished. success={success}, error={error}"));
    }

    private void HandleStoreConnected()
    {
        Debug.Log("[IAP] Store connected.");
        IsReady = true;
        LastError = null;

        var defs = new List<ProductDefinition>(catalog.Entries.Count);
        for (int i = 0; i < catalog.Entries.Count; i++)
        {
            var e = catalog.Entries[i];
            defs.Add(new ProductDefinition(e.productId, ToUnityType(e.type)));
        }

        _store.FetchProducts(defs);
    }

    private void HandleStoreDisconnected(StoreConnectionFailureDescription failure)
    {
        IsReady = false;
        FailInit($"Store disconnected: {failure?.message}");
    }

    private void HandleProductsFetched(List<Product> products)
    {
        _products.Clear();
        foreach (var p in products)
        {
            _products[p.definition.id] = p;

            OnProductFetched?.Invoke(new IapProductInfo(
                p.definition.id,
                p.metadata.localizedTitle,
                p.metadata.localizedDescription,
                p.metadata.localizedPriceString,
                p.availableToPurchase));
        }

        OnInitialized?.Invoke();
    }

    private void HandleProductsFetchFailed(ProductFetchFailed failure)
    {
        string detail = failure != null ? failure.ToString() : "unknown";

        Debug.LogError($"[IAP] Products fetch failed: {detail}");

        OnInitializeFailed?.Invoke("Product fetch failed. See console for details.");
    }

    private void HandlePurchasePending(PendingOrder order)
    {
        var (productId, transactionId, receipt) = ExtractOrder(order);
        if (string.IsNullOrEmpty(productId))
        {
            Debug.LogError("[IAP] Pending order has no product id — confirming to unblock store.");
            _store.ConfirmPurchase(order);
            return;
        }

        Debug.Log($"[IAP] Purchase pending: {productId}");
        _pendingOrders[productId] = order;

        OnPurchaseSucceeded?.Invoke(new IapPurchaseInfo(productId, transactionId, receipt));
    }

    private void HandlePurchaseConfirmed(Order order)
        => Debug.Log($"[IAP] Purchase confirmed by store: {ExtractOrder(order).ProductId}");

    private void HandlePurchaseFailed(FailedOrder order)
    {
        var (productId, _, _) = ExtractOrder(order);
        string reason = order?.FailureReason.ToString() ?? "Unknown";
        string message = order?.Details ?? reason;

        Debug.LogWarning($"[IAP] Purchase failed: {productId} — {reason}: {message}");
        OnPurchaseFailed?.Invoke(new IapPurchaseFailure(productId, reason, message));
    }

    private static (string ProductId, string TransactionId, string Receipt) ExtractOrder(Order order)
    {
        if (order?.CartOrdered == null) return ("", "", "");

        var items = order.CartOrdered.Items();
        if (items == null || items.Count == 0) return ("", "", "");

        var product = items[0].Product;
        string id = product?.definition?.id ?? "";
        return (id, order.Info?.TransactionID ?? "", order.Info?.Receipt ?? "");
    }

    private void FailInit(string message)
    {
        LastError = message;
        Debug.LogError($"[IAP] Init failed. {message}");
        OnInitializeFailed?.Invoke(message);
    }

    private void EmitFailure(string productId, string reason, string message)
    {
        Debug.LogWarning($"[IAP] {message} ({productId})");
        OnPurchaseFailed?.Invoke(new IapPurchaseFailure(productId, reason, message));
    }

    private static ProductType ToUnityType(IapProductType t) => t switch
    {
        IapProductType.Consumable => ProductType.Consumable,
        IapProductType.NonConsumable => ProductType.NonConsumable,
        IapProductType.Subscription => ProductType.Subscription,
        _ => ProductType.Consumable
    };
}