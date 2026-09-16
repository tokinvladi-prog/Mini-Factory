using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IapCatalog", menuName = "Scriptable Objects/IAP Catalog")]
public class IapCatalog : ScriptableObject
{
    [SerializeField] private List<IapProductDefinition> entries = new();

    public IReadOnlyList<IapProductDefinition> Entries => entries;

    public bool TryGet(string productId, out IapProductDefinition def)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].productId == productId) { def = entries[i]; return true; }
        }
        def = null;
        return false;
    }
}