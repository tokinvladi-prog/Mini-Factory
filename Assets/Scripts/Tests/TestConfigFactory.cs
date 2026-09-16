using UnityEditor;
using UnityEngine;

internal static class TestConfigFactory
{
    public static MachineConfig Machine(
        string id,
        double baseIncome = 1d,
        double baseUnlockCost = 10d,
        double baseUpgradeCost = 5d,
        float costMultiplier = 1.15f)
    {
        var c = ScriptableObject.CreateInstance<MachineConfig>();
        var so = new SerializedObject(c);
        so.FindProperty("id").stringValue = id;
        so.FindProperty("baseIncome").doubleValue = baseIncome;
        so.FindProperty("baseUnlockCost").doubleValue = baseUnlockCost;
        so.FindProperty("baseUpgradeCost").doubleValue = baseUpgradeCost;
        so.FindProperty("costMultiplier").floatValue = costMultiplier;
        so.ApplyModifiedPropertiesWithoutUndo();
        return c;
    }

    public static FactoryConfig Factory(
        float boostMultiplier = 2f,
        float boostDuration = 30f,
        float maxOfflineSeconds = 3600f,
        params MachineConfig[] machines)
    {
        var c = ScriptableObject.CreateInstance<FactoryConfig>();
        var so = new SerializedObject(c);
        so.FindProperty("boostMultiplier").floatValue = boostMultiplier;
        so.FindProperty("boostDuration").floatValue = boostDuration;
        so.FindProperty("maxOfflineSeconds").floatValue = maxOfflineSeconds;

        var list = so.FindProperty("machines");
        list.arraySize = machines.Length;
        for (int i = 0; i < machines.Length; i++)
            list.GetArrayElementAtIndex(i).objectReferenceValue = machines[i];

        so.ApplyModifiedPropertiesWithoutUndo();
        return c;
    }
}
