using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Project.UI
{
    /// <summary>
    /// Widok pojedynczej pozycji towaru NPC (lewa kolumna panelu handlu). Czysty "widok" - nie zna
    /// NpcMerchant/TradeRuntimeSlot, tylko wyświetla przekazane dane i przekazuje kliknięcie przez
    /// podpięty callback, tak samo jak QuestListEntryUI. Przycisk Kup jest wyszarzony (interactable
    /// = false), gdy quantity == 0, zamiast być ukrywany - gracz wciąż widzi, że NPC handlował tym
    /// towarem, tylko akurat wyprzedanym.
    /// </summary>
    public class TradeStockEntryUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private TextMeshProUGUI quantityText;
        [SerializeField] private Button buyButton;

        public void Setup(string itemName, int price, int quantity, UnityAction onBuyClicked)
        {
            nameText.text = itemName;
            priceText.text = price + "z";
            quantityText.text = "x" + quantity;

            buyButton.interactable = quantity > 0;
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(onBuyClicked);
        }
    }
}
