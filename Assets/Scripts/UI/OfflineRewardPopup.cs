using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OfflineRewardPopup : MonoBehaviour
{
    [SerializeField] private FactoryManager factory;
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private Button collectButton;

    private void Awake()
    {
        root.SetActive(false);
        collectButton.onClick.AddListener(HandleCollect);
    }

    private void Start()
    {
        factory.OnOfflineRewardReady += HandleRewardReady;
        if (factory.HasPendingOfflineReward)
            Show(factory.PendingOfflineReward);
    }

    private void OnDestroy()
    {
        factory.OnOfflineRewardReady -= HandleRewardReady;
        collectButton.onClick.RemoveListener(HandleCollect);
    }

    private void HandleRewardReady(float amount) => Show(amount);

    private void Show(float amount)
    {
        rewardText.text = NumberFormat.Short(amount);
        root.SetActive(true);
    }

    private void HandleCollect()
    {
        factory.CollectOfflineReward();
        root.SetActive(false);
    }
}
