using System;
using Project.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.UI
{
    /// <summary>
    /// Panel podglądu wybranego przedmiotu (ikona, nazwa, opis, kategoria, przycisk "Załóż").
    /// Czysty "widok" - nie zna InventorySystem/EquipmentSystem ani slotów, tylko wyświetla
    /// przekazany ItemData i zgłasza kliknięcie przycisku "Załóż" przez OnEquipClicked;
    /// samo wywołanie EquipmentSystem.TryEquip leży po stronie InventoryUIController.
    /// </summary>
    public class ItemDescriptionPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI typeText;
        [SerializeField] private Button equipButton;

        private ItemData _currentItem;

        /// <summary>Wywoływane po kliknięciu przycisku "Załóż" dla aktualnie wyświetlanego przedmiotu.</summary>
        public event Action<ItemData> OnEquipClicked;

        private void Awake()
        {
            if (equipButton != null)
            {
                equipButton.onClick.AddListener(HandleEquipClicked);
            }
        }

        private void Start()
        {
            panelRoot.SetActive(false);
        }

        public void Show(ItemData item)
        {
            _currentItem = item;

            iconImage.sprite = item.Icon;
            iconImage.enabled = item.Icon != null;
            nameText.text = item.DisplayName;
            descriptionText.text = item.Description;

            if (typeText != null)
            {
                typeText.text = item.ItemType.ToString();
            }

            if (equipButton != null)
            {
                bool isEquippable = item.ItemType == ItemType.Weapon || item.ItemType == ItemType.Armor || item.ItemType == ItemType.Jewelry;
                equipButton.gameObject.SetActive(isEquippable);
            }

            panelRoot.SetActive(true);
        }

        public void Hide()
        {
            _currentItem = null;
            panelRoot.SetActive(false);
        }

        private void HandleEquipClicked()
        {
            if (_currentItem != null)
            {
                OnEquipClicked?.Invoke(_currentItem);
            }
        }
    }
}
