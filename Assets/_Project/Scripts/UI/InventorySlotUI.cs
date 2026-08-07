using System;
using Project.Data;
using Project.Gameplay.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Project.UI
{
    /// <summary>
    /// Widok pojedynczego slotu ekwipunku. Czysty "widok" - nie zna InventorySystem,
    /// tylko wyświetla dane przekazane przez InventoryUIController i zgłasza kliknięcia.
    /// </summary>
    public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI quantityText;

        private ItemData _currentItem;

        /// <summary>Wywoływane po kliknięciu slotu. Dla pustego slotu przekazuje null.</summary>
        public event Action<ItemData> OnSlotClicked;

        /// <summary>Wypełnia slot danymi z zajętego InventorySlot.</summary>
        public void SetSlot(InventorySlot slot)
        {
            _currentItem = slot.Item;

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
            _currentItem = null;

            iconImage.enabled = false;
            iconImage.sprite = null;
            quantityText.enabled = false;
            quantityText.text = string.Empty;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnSlotClicked?.Invoke(_currentItem);
        }
    }
}
