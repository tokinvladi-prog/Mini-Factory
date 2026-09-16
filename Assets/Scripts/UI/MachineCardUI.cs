using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MachineCardUI : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text incomeText;
    [SerializeField] private TMP_Text priceText;

    [Header("Action")]
    [SerializeField] private Button actionButton;
    [SerializeField] private TMP_Text actionButtonLabel;

    private FactoryManager _factory;
    private MachineModel _model;

    public void Bind(FactoryManager factory, MachineModel model)
    {
        Unbind();

        _factory = factory;
        _model = model;

        _factory.OnBalanceChanged += HandleBalanceChanged;
        _factory.OnMachineChanged += HandleMachineChanged;
        actionButton.onClick.AddListener(HandleClick);

        RefreshContent();
        RefreshAffordability();
    }

    private void OnDestroy() => Unbind();

    private void Unbind()
    {
        if (_factory != null)
        {
            _factory.OnBalanceChanged -= HandleBalanceChanged;
            _factory.OnMachineChanged -= HandleMachineChanged;
        }
        if (actionButton != null)
            actionButton.onClick.RemoveListener(HandleClick);

        _factory = null;
        _model = null;
    }

    private void HandleClick()
    {
        if (!_model.IsUnlocked) _factory.TryUnlock(_model);
        else _factory.TryUpgrade(_model);
    }

    private void HandleMachineChanged(MachineModel changed)
    {
        if (!ReferenceEquals(changed, _model)) return;
        RefreshContent();
        RefreshAffordability();
    }

    private void HandleBalanceChanged(float _) => RefreshAffordability();

    private void RefreshContent()
    {
        nameText.text = _model.Config.Id;
        levelText.text = _model.IsUnlocked ? $"Lvl {_model.Level}" : "Locked";
        incomeText.text = $"{NumberFormat.Short(_model.GetIncome())}/s";
    }

    private void RefreshAffordability()
    {
        float price = _model.IsUnlocked ? _model.GetUpgradeCost() : _model.Config.BaseUnlockCost;
        float balance = _factory.Model.Balance;

        bool affordable = _model.IsUnlocked
            ? _model.CanUpgrade(balance)
            : _model.CanUnlock(balance);

        priceText.text = NumberFormat.Short(price);
        actionButtonLabel.text = _model.IsUnlocked ? "Upgrade" : "Buy";
        actionButton.interactable = affordable;
    }
}
