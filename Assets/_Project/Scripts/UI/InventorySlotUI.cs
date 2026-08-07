using Project.Gameplay.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.UI
{
    /// <summary>
    /// Widok pojedynczego slotu ekwipunku. Czysty "widok" - nie zna InventorySystem,
    /// tylko wyświetla dane przekazane przez InventoryUIController.
    /// </summary>
    public class InventorySlotUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI quantityText;

        /// <summary>Wypełnia slot danymi z zajętego InventorySlot.</summary>
        public void SetSlot(InventorySlot slot)
        {
            iconImage.enabled = true;
            iconImage.sprite = slot.Item.Icon;

            if (slot.Quantity > 1)
            {
                quantityText.enabled = true;
                quantityText.text = slot.Quantity.ToString();
            }
            else
            {
                quantityText.enabled = false;
                quantityText.text = string.Empty;
            }
        }

        /// <summary>Czyści slot do stanu pustego placeholdera (bez ikony, bez licznika).</summary>
        public void SetEmpty()
        {
            iconImage.enabled = false;
            iconImage.sprite = null;
            quantityText.enabled = false;
            quantityText.text = string.Empty;
        }
    }
}
