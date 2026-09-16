using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopButtonUI : MonoBehaviour
{
    [SerializeField] private IapCoordinator iap;
    [SerializeField] private string productId = IapProductIds.CoinsPackSmall;
    [SerializeField] private Button buyButton;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text statusText;

    private void Start()
    {
        buyButton.onClick.AddListener(HandleBuy);

        iap.OnReady += HandleReady;
        iap.OnFailed += HandleFailed;
        iap.OnPurchaseCompleted += HandleCompleted;

        if (iap.TryGetProduct(productId, out var info)) ApplyProduct(info);

        bool ready = iap.IsReady;
        buyButton.interactable = ready;
        statusText.text = ready ? "" : "Connecting to store…";
    }

    private void OnDestroy()
    {
        buyButton.onClick.RemoveListener(HandleBuy);
        iap.OnReady -= HandleReady;
        iap.OnFailed -= HandleFailed;
        iap.OnPurchaseCompleted -= HandleCompleted;
    }

    private void HandleBuy()
    {
        statusText.text = "";
        iap.TryBuy(productId);
    }

    private void HandleReady()
    {
        buyButton.interactable = true;
        statusText.text = "";
    }

    private void HandleFailed(string error)
    {
        buyButton.interactable = false;
        statusText.text = "Store unavailable";
        Debug.LogWarning($"[Shop] {error}");
    }

    private void HandleCompleted(IapPurchaseInfo info)
    {
        if (info.ProductId != productId) return;
        statusText.text = "Thanks!";
    }

    private void ApplyProduct(IapProductInfo info)
    {
        priceText.text = string.IsNullOrEmpty(info.LocalizedPrice) ? "—" : info.LocalizedPrice;
        buyButton.interactable = info.AvailableToPurchase && iap.IsReady;
    }
}