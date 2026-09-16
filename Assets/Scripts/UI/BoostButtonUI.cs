using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoostButtonUI : MonoBehaviour
{
    [SerializeField] private FactoryManager factory;
    [SerializeField] private Button activateButton;
    [SerializeField] private TMP_Text buttonLabel;
    [SerializeField] private GameObject timerRoot;
    [SerializeField] private TMP_Text timerText;

    private void Start()
    {
        activateButton.onClick.AddListener(HandleClick);
        factory.Boost.OnTimeChanged += HandleTimeChanged;
        factory.Boost.OnExpired += HandleExpired;

        Refresh(factory.Boost.TimeRemaining);
    }

    private void OnDestroy()
    {
        activateButton.onClick.RemoveListener(HandleClick);
        factory.Boost.OnTimeChanged -= HandleTimeChanged;
        factory.Boost.OnExpired -= HandleExpired;
    }

    private void HandleClick() => factory.TryActivateBoost();
    private void HandleTimeChanged(float t) => Refresh(t);
    private void HandleExpired() => Refresh(0f);

    private void Refresh(float t)
    {
        bool active = t > 0f;

        timerRoot.SetActive(active);
        activateButton.interactable = !active;
        buttonLabel.text = active ? "x2" : "Activate Boost";

        if (!active) return;

        int sec = Mathf.CeilToInt((float)t);
        int m = sec / 60;
        int s = sec % 60;
        timerText.text = $"{m:00}:{s:00}";
    }
}
