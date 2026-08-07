using System.Collections.Generic;
using Project.Core.Events;
using Project.Gameplay.Inventory;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.UI
{
    /// <summary>
    /// Podstawowy panel UI ekwipunku - tylko podgląd (bez drag&amp;drop, bez używania przedmiotów).
    /// Przełączany akcją ToggleInventory, odświeżany po InventoryChangedEvent oraz przy otwarciu
    /// (na wypadek zmian, które zaszły podczas gdy panel był zamknięty).
    /// </summary>
    public class InventoryUIController : MonoBehaviour
    {
        [SerializeField] private InventoryBridge inventoryBridge;
        [SerializeField] private GameObject inventoryPanelRoot;
        [SerializeField] private Transform slotsContainer;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private InputActionReference toggleInventoryAction;

        private void Awake()
        {
            inventoryPanelRoot.SetActive(false);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<InventoryChangedEvent>(OnInventoryChanged);

            toggleInventoryAction.action.Enable();
            toggleInventoryAction.action.performed += OnTogglePerformed;
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<InventoryChangedEvent>(OnInventoryChanged);

            toggleInventoryAction.action.performed -= OnTogglePerformed;
            toggleInventoryAction.action.Disable();
        }

        private void OnInventoryChanged(InventoryChangedEvent evt)
        {
            if (inventoryPanelRoot.activeSelf)
            {
                RefreshSlots();
            }
        }

        private void OnTogglePerformed(InputAction.CallbackContext context)
        {
            bool willBeActive = !inventoryPanelRoot.activeSelf;
            inventoryPanelRoot.SetActive(willBeActive);

            if (willBeActive)
            {
                RefreshSlots();
            }
        }

        private void RefreshSlots()
        {
            int childCount = slotsContainer.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                Destroy(slotsContainer.GetChild(i).gameObject);
            }

            IReadOnlyList<InventorySlot> slots = inventoryBridge.Inventory.Slots;
            for (int i = 0; i < slots.Count; i++)
            {
                InventorySlot slot = slots[i];
                GameObject slotInstance = Instantiate(slotPrefab, slotsContainer);
                InventorySlotUI slotUI = slotInstance.GetComponent<InventorySlotUI>();

                if (slot.IsEmpty)
                {
                    slotUI.SetEmpty();
                }
                else
                {
                    slotUI.SetSlot(slot);
                }
            }
        }
    }
}
