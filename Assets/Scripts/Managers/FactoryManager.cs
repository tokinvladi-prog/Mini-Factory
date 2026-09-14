using UnityEngine;

public class FactoryManager : MonoBehaviour
{
    [SerializeField] private FactoryConfig config;
    [SerializeField] private float startingBalance = 0f;

    public FactoryModel Model { get; private set; }

    private void Awake()
    {
        Model = new FactoryModel(config, startingBalance);
    }

    private void Update()
    {
        if (Model == null) return;
        Model.Tick(Time.deltaTime);
    }
}
