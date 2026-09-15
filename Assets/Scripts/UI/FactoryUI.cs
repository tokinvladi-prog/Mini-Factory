using TMPro;
using UnityEngine;

    public class FactoryUI : MonoBehaviour
    {
        [SerializeField] private FactoryManager factory;
        [SerializeField] private MachineCardUI cardPrefab;
        [SerializeField] private Transform cardsRoot;
        [SerializeField] private TMP_Text balanceText;
        [SerializeField] private TMP_Text incomeText;

        private void Start()
        {
            foreach (var machine in factory.Model.Machines)
            {
                var card = Instantiate(cardPrefab, cardsRoot);
                card.Bind(factory, machine);
            }

            factory.OnBalanceChanged += HandleBalanceChanged;
            factory.OnIncomeChanged += HandleIncomeChanged;

            HandleBalanceChanged(factory.Model.Balance);
            HandleIncomeChanged(factory.Model.TotalIncomePerSecond);
        }

        private void OnDestroy()
        {
            if (factory == null) return;
            factory.OnBalanceChanged -= HandleBalanceChanged;
            factory.OnIncomeChanged -= HandleIncomeChanged;
        }

        private void HandleBalanceChanged(float v) => balanceText.text = NumberFormat.Short(v);
        private void HandleIncomeChanged(float v) => incomeText.text = $"{NumberFormat.Short(v)}/s";
    }
    