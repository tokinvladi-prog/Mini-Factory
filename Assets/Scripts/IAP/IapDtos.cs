public readonly struct IapProductInfo
{
    public readonly string ProductId;
    public readonly string Title;
    public readonly string Description;
    public readonly string LocalizedPrice;
    public readonly bool AvailableToPurchase;

    public IapProductInfo(string productId, string title, string description,
                          string localizedPrice, bool availableToPurchase)
    {
        ProductId = productId;
        Title = title;
        Description = description;
        LocalizedPrice = localizedPrice;
        AvailableToPurchase = availableToPurchase;
    }
}

public readonly struct IapPurchaseInfo
{
    public readonly string ProductId;
    public readonly string TransactionId;
    public readonly string Receipt;

    public IapPurchaseInfo(string productId, string transactionId, string receipt)
    {
        ProductId = productId;
        TransactionId = transactionId;
        Receipt = receipt;
    }
}

public readonly struct IapPurchaseFailure
{
    public readonly string ProductId;
    public readonly string Reason;
    public readonly string Message;

    public IapPurchaseFailure(string productId, string reason, string message)
    {
        ProductId = productId;
        Reason = reason;
        Message = message;
    }
}