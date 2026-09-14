using UnityEngine;

[RequireComponent(typeof(FactoryManager))]
public class FactoryDebugTester : MonoBehaviour
{
    [SerializeField] private float logInterval = 1f;
    private FactoryManager _manager;
    private float _timer;

    private void Awake() => _manager = GetComponent<FactoryManager>();

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer < logInterval) return;
        _timer = 0f;

        var m = _manager.Model;
        Debug.Log($"[Factory] Balance={m.Balance:F2} Income/sec={m.TotalIncomePerSecond:F2}");

        var first = m.Machines.Count > 0 ? m.Machines[0] : null;
        if (!first.IsUnlocked) m.TryUnlock(first);
        else m.TryUpgrade(first);
    }
}
