using UnityEngine;

[CreateAssetMenu(fileName = "MachineConfig", menuName = "Scriptable Objects/MachineConfig")]
public class MachineConfig : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private float baseIncome;
    [SerializeField] private float baseUnlockCost;
    [SerializeField] private float baseUpgradeCost;
    [SerializeField] private float costMultiplier;

    public string Id => id;
    public float BaseIncome => baseIncome;
    public float BaseUnlockCost => baseUnlockCost;
    public float BaseUpgradeCost => baseUpgradeCost;
    public float CostMultiplier => costMultiplier;
}
