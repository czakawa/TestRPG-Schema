using Project.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.UI
{
    /// <summary>
    /// Panel podglądu wybranego przedmiotu (ikona, nazwa, opis, kategoria). Czysty "widok" -
    /// nie zna InventorySystem ani slotów, tylko wyświetla przekazany ItemData.
    /// </summary>
    public class ItemDescriptionPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI typeText;

        private void Start()
        {
            panelRoot.SetActive(false);
        }

        public void Show(ItemData item)
        {
            iconImage.sprite = item.Icon;
            iconImage.enabled = item.Icon != null;
            nameText.text = item.DisplayName;
            descriptionText.text = item.Description;

            if (typeText != null)
            {
                typeText.text = item.ItemType.ToString();
            }

            panelRoot.SetActive(true);
        }

        public void Hide()
        {
            panelRoot.SetActive(false);
        }
    }
}
