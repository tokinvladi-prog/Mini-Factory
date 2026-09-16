using System;

public enum IapProductType { Consumable, NonConsumable, Subscription }

[Serializable]
public class IapProductDefinition
{
    public string productId;
    public IapProductType type = IapProductType.Consumable;
    public float currencyReward = 1000f;
}
