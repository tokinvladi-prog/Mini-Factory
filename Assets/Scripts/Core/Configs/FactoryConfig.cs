using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FactoryConfig", menuName = "Scriptable Objects/FactoryConfig")]
public class FactoryConfig : ScriptableObject
{
    [SerializeField] private List<MachineConfig> machines;
    [SerializeField] private float boostMultiplier;
    [SerializeField] private float boostDuration;
    [SerializeField] private float maxOfflineSeconds;

    public IReadOnlyList<MachineConfig> Machines => machines;
    public float BoostMultiplier => boostMultiplier;
    public float BoostDuration => boostDuration;
    public float MaxOfflineSeconds => maxOfflineSeconds;
}
