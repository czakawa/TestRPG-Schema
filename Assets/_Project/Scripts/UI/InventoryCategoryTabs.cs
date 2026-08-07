using System;
using System.Collections.Generic;
using Project.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.UI
{
    /// <summary>
    /// Generuje zakładki kategorii ("Wszystkie" + jedna na każdą wartość ItemType) i wystawia
    /// lokalne C# zdarzenie OnCategorySelected. To czysto lokalny stan UI (który filtr jest aktywny),
    /// nie trafia do globalnego EventBus - subskrybuje go bezpośrednio InventoryUIController.
    /// </summary>
    public class InventoryCategoryTabs : MonoBehaviour
    {
        private static readonly Color SelectedColor = new Color(0.85f, 0.65f, 0.13f);
        private static readonly Color UnselectedColor = Color.white;

        [SerializeField] private Transform tabsContainer;
        [SerializeField] private GameObject tabButtonPrefab;

        private readonly List<Button> _tabButtons = new List<Button>();
        private readonly List<ItemType?> _tabCategories = new List<ItemType?>();

        /// <summary>Wywoływane po kliknięciu zakładki. null = "Wszystkie" (brak filtra).</summary>
        public event Action<ItemType?> OnCategorySelected;

        private void Awake()
        {
            BuildTabs();
        }

        private void BuildTabs()
        {
            CreateTab("Wszystkie", null);

            foreach (ItemType itemType in (ItemType[])Enum.GetValues(typeof(ItemType)))
            {
                CreateTab(itemType.ToString(), itemType);
            }

            SelectTab(0);
        }

        private void CreateTab(string label, ItemType? category)
        {
            GameObject instance = Instantiate(tabButtonPrefab, tabsContainer);

            TextMeshProUGUI labelText = instance.GetComponentInChildren<TextMeshProUGUI>();
            if (labelText != null)
            {
                labelText.text = label;
            }

            Button button = instance.GetComponent<Button>();
            int index = _tabButtons.Count;
            button.onClick.AddListener(() => SelectTab(index));

            _tabButtons.Add(button);
            _tabCategories.Add(category);
        }

        private void SelectTab(int index)
        {
            for (int i = 0; i < _tabButtons.Count; i++)
            {
                SetTabHighlighted(_tabButtons[i], i == index);
            }

            OnCategorySelected?.Invoke(_tabCategories[index]);
        }

        private static void SetTabHighlighted(Button button, bool highlighted)
        {
            ColorBlock colors = button.colors;
            Color color = highlighted ? SelectedColor : UnselectedColor;
            colors.normalColor = color;
            colors.selectedColor = color;
            button.colors = colors;
        }
    }
}
