using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Project.UI
{
    /// <summary>
    /// Widok pojedynczego przedmiotu gracza do sprzedaży (prawa kolumna panelu handlu). Czysty
    /// "widok" - nie zna InventorySystem/ItemData poza tym, co dostaje w Setup. Przycisk Sprzedaj
    /// jest wyszarzony dla przedmiotów z BasePrice &lt;= 0 (niesprzedawalnych), żeby gracz nie klikał
    /// martwego przycisku - NpcMerchant.TrySell i tak by to odrzucił, ale bez wizualnej informacji.
    /// </summary>
    public class TradeSellEntryUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI quantityText;
        [SerializeField] private Button sellButton;

        public void Setup(string itemName, int quantity, bool canSell, UnityAction onSellClicked)
        {
            nameText.text = itemName;
            quantityText.text = "x" + quantity;

            sellButton.interactable = canSell;
            sellButton.onClick.RemoveAllListeners();
            sellButton.onClick.AddListener(onSellClicked);
        }
    }
}
