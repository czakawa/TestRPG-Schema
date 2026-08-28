using System.Collections.Generic;
using Project.Core;
using Project.Core.Events;
using Project.Core.States;
using Project.Data;
using Project.Gameplay.Equipment;
using Project.Gameplay.Inventory;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.UI
{
    /// <summary>
    /// Podstawowy panel UI ekwipunku - podgląd z filtrowaniem po kategorii i podglądem opisu
    /// (bez drag&amp;drop, bez używania przedmiotów). Przełączany akcją ToggleInventory,
    /// odświeżany po InventoryChangedEvent oraz przy otwarciu (na wypadek zmian, które zaszły
    /// podczas gdy panel był zamknięty). Blokuje ruch i kamerę gracza przez GameplayInputLock,
    /// dopóki jest otwarty - PlayerMotorBridge/CameraOrbitBridge czytają input w Update(), poza
    /// pętlą GameSystemsManager, więc nie ma tu żadnego istniejącego mechanizmu stanu gry, który
    /// by to załatwił. Nie da się otworzyć w trakcie stanu Dialogue, a jeśli dialog rozpocznie się
    /// podczas gdy panel jest już otwarty (możliwe, bo otwarcie ekwipunku nie blokuje interakcji -
    /// patrz InteractionSystem), panel zamyka się automatycznie po DialogueStartedEvent.
    /// </summary>
    public class InventoryUIController : MonoBehaviour
    {
        private const string InventoryLockReason = "Inventory";

        [SerializeField] private InventoryBridge inventoryBridge;
        [SerializeField] private EquipmentBridge equipmentBridge;
        [SerializeField] private GameObject inventoryPanelRoot;
        [SerializeField] private Transform slotsContainer;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private InputActionReference toggleInventoryAction;
        [SerializeField] private InventoryCategoryTabs categoryTabs;
        [SerializeField] private ItemDescriptionPanel descriptionPanel;
        [SerializeField] private EquipmentSlotUI weaponSlot;
        [SerializeField] private EquipmentSlotUI armorSlot;
        [SerializeField] private EquipmentSlotUI ring1Slot;
        [SerializeField] private EquipmentSlotUI ring2Slot;
        [SerializeField] private EquipmentSlotUI necklaceSlot;

        private ItemType? _activeFilter;

        private void Awake()
        {
            inventoryPanelRoot.SetActive(false);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<InventoryChangedEvent>(OnInventoryChanged);
            EventBus.Subscribe<DialogueStartedEvent>(OnDialogueStarted);
            EventBus.Subscribe<EquipmentChangedEvent>(OnEquipmentChanged);

            toggleInventoryAction.action.Enable();
            toggleInventoryAction.action.performed += OnTogglePerformed;

            categoryTabs.OnCategorySelected += OnCategorySelected;
            descriptionPanel.OnEquipClicked += OnEquipClicked;

            weaponSlot.OnSlotClicked += OnWeaponSlotClicked;
            armorSlot.OnSlotClicked += OnArmorSlotClicked;
            ring1Slot.OnSlotClicked += OnRing1SlotClicked;
            ring2Slot.OnSlotClicked += OnRing2SlotClicked;
            necklaceSlot.OnSlotClicked += OnNecklaceSlotClicked;
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<InventoryChangedEvent>(OnInventoryChanged);
            EventBus.Unsubscribe<DialogueStartedEvent>(OnDialogueStarted);
            EventBus.Unsubscribe<EquipmentChangedEvent>(OnEquipmentChanged);

            toggleInventoryAction.action.performed -= OnTogglePerformed;
            toggleInventoryAction.action.Disable();

            categoryTabs.OnCategorySelected -= OnCategorySelected;
            descriptionPanel.OnEquipClicked -= OnEquipClicked;

            weaponSlot.OnSlotClicked -= OnWeaponSlotClicked;
            armorSlot.OnSlotClicked -= OnArmorSlotClicked;
            ring1Slot.OnSlotClicked -= OnRing1SlotClicked;
            ring2Slot.OnSlotClicked -= OnRing2SlotClicked;
            necklaceSlot.OnSlotClicked -= OnNecklaceSlotClicked;

            if (inventoryPanelRoot.activeSelf)
            {
                GameplayInputLock.UnlockMovement(InventoryLockReason);
                GameplayInputLock.UnlockCamera(InventoryLockReason);
            }
        }

        private void OnInventoryChanged(InventoryChangedEvent evt)
        {
            if (inventoryPanelRoot.activeSelf)
            {
                RefreshSlots();
            }
        }

        private void OnEquipmentChanged(EquipmentChangedEvent evt)
        {
            if (inventoryPanelRoot.activeSelf)
            {
                RefreshEquipment();
            }
        }

        private void OnTogglePerformed(InputAction.CallbackContext context)
        {
            if (inventoryPanelRoot.activeSelf)
            {
                ClosePanel();
                return;
            }

            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameStateType.Dialogue)
            {
                Debug.Log("Ekwipunek jest niedostępny podczas rozmowy.");
                return;
            }

            OpenPanel();
        }

        private void OnDialogueStarted(DialogueStartedEvent evt)
        {
            if (inventoryPanelRoot.activeSelf)
            {
                ClosePanel();
            }
        }

        private void OpenPanel()
        {
            inventoryPanelRoot.SetActive(true);
            GameplayInputLock.LockMovement(InventoryLockReason);
            GameplayInputLock.LockCamera(InventoryLockReason);
            RefreshSlots();
            RefreshEquipment();
        }

        private void ClosePanel()
        {
            inventoryPanelRoot.SetActive(false);
            GameplayInputLock.UnlockMovement(InventoryLockReason);
            GameplayInputLock.UnlockCamera(InventoryLockReason);
            descriptionPanel.Hide();
        }

        private void OnCategorySelected(ItemType? category)
        {
            _activeFilter = category;
            RefreshSlots();
        }

        private void OnSlotClicked(ItemData item)
        {
            if (item != null)
            {
                descriptionPanel.Show(item);
            }
            else
            {
                descriptionPanel.Hide();
            }
        }

        private void OnEquipClicked(ItemData item)
        {
            if (equipmentBridge.Equipment.TryEquip(item))
            {
                descriptionPanel.Hide();
            }
        }

        private void OnWeaponSlotClicked()
        {
            equipmentBridge.Equipment.TryUnequip(EquipmentSlot.Weapon);
        }

        private void OnArmorSlotClicked()
        {
            equipmentBridge.Equipment.TryUnequip(EquipmentSlot.Armor);
        }

        private void OnRing1SlotClicked()
        {
            equipmentBridge.Equipment.TryUnequip(EquipmentSlot.Ring1);
        }

        private void OnRing2SlotClicked()
        {
            equipmentBridge.Equipment.TryUnequip(EquipmentSlot.Ring2);
        }

        private void OnNecklaceSlotClicked()
        {
            equipmentBridge.Equipment.TryUnequip(EquipmentSlot.Necklace);
        }

        private void RefreshEquipment()
        {
            EquipmentSystem equipment = equipmentBridge.Equipment;

            weaponSlot.SetItem(equipment.EquippedWeapon);
            armorSlot.SetItem(equipment.EquippedArmor);
            ring1Slot.SetItem(equipment.EquippedRing1);
            ring2Slot.SetItem(equipment.EquippedRing2);
            necklaceSlot.SetItem(equipment.EquippedNecklace);
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

                if (_activeFilter.HasValue && (slot.IsEmpty || slot.Item.ItemType != _activeFilter.Value))
                {
                    continue;
                }

                GameObject slotInstance = Instantiate(slotPrefab, slotsContainer);
                InventorySlotUI slotUI = slotInstance.GetComponent<InventorySlotUI>();
                slotUI.OnSlotClicked += OnSlotClicked;

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
