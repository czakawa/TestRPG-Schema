using System;
using Project.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Project.UI
{
    /// <summary>
    /// Widok pojedynczego slotu ekwipunku zakładanego (broń/zbroja/biżuteria). Czysty "widok" -
    /// nie zna EquipmentSystem ani typu slotu, tylko wyświetla przekazaną ikonę i zgłasza
    /// kliknięcie. CharacterPanelUI decyduje, który ItemType odpowiada danej instancji slotu.
    /// </summary>
    public class EquipmentSlotUI : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image iconImage;

        /// <summary>Wywoływane po kliknięciu slotu, niezależnie od tego czy jest pusty czy zajęty.</summary>
        public event Action OnSlotClicked;

        /// <summary>Ustawia ikonę slotu. item == null pokazuje pusty placeholder.</summary>
        public void SetItem(ItemData item)
        {
            if (item == null)
            {
                iconImage.enabled = false;
                iconImage.sprite = null;
                return;
            }

            iconImage.enabled = true;
            iconImage.sprite = item.Icon;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnSlotClicked?.Invoke();
        }
    }
}
