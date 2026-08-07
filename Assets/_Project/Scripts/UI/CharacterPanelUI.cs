using Project.Core;
using Project.Core.Events;
using Project.Core.States;
using Project.Data;
using Project.Gameplay.Equipment;
using Project.Gameplay.Stats;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.UI
{
    /// <summary>
    /// Panel postaci - podgląd statystyk (Vitale + atrybuty) i trzech slotów ekwipunku zakładanego.
    /// Przełączany akcją ToggleCharacterPanel (klawisz C), odświeżany po StatsChangedEvent
    /// i EquipmentChangedEvent oraz przy otwarciu, dokładnie jak InventoryUIController.
    ///
    /// Blokuje ruch/kamerę własnym kluczem "CharacterPanel" w GameplayInputLock, niezależnym od
    /// klucza "Inventory" - panel postaci i panel ekwipunku (Tab) mogą być otwarte jednocześnie
    /// (sensowny use case: gracz porównuje założony sprzęt z zawartością plecaka), każdy panel
    /// niezależnie zakłada i zdejmuje własną blokadę. Ten sam guard na stan Dialogue i ten sam
    /// wzorzec auto-zamknięcia po DialogueStartedEvent co InventoryUIController.
    /// </summary>
    public class CharacterPanelUI : MonoBehaviour
    {
        private const string CharacterPanelLockReason = "CharacterPanel";

        [SerializeField] private PlayerStatsBridge playerStatsBridge;
        [SerializeField] private EquipmentBridge equipmentBridge;

        [SerializeField] private GameObject characterPanelRoot;
        [SerializeField] private InputActionReference toggleCharacterPanelAction;

        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI staminaText;
        [SerializeField] private TextMeshProUGUI manaText;

        [SerializeField] private TextMeshProUGUI strengthText;
        [SerializeField] private TextMeshProUGUI dexterityText;
        [SerializeField] private TextMeshProUGUI enduranceText;
        [SerializeField] private TextMeshProUGUI wisdomText;

        [SerializeField] private EquipmentSlotUI weaponSlot;
        [SerializeField] private EquipmentSlotUI armorSlot;
        [SerializeField] private EquipmentSlotUI jewelrySlot;

        private void Awake()
        {
            characterPanelRoot.SetActive(false);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<StatsChangedEvent>(OnStatsChanged);
            EventBus.Subscribe<EquipmentChangedEvent>(OnEquipmentChanged);
            EventBus.Subscribe<DialogueStartedEvent>(OnDialogueStarted);

            toggleCharacterPanelAction.action.Enable();
            toggleCharacterPanelAction.action.performed += OnTogglePerformed;

            weaponSlot.OnSlotClicked += OnWeaponSlotClicked;
            armorSlot.OnSlotClicked += OnArmorSlotClicked;
            jewelrySlot.OnSlotClicked += OnJewelrySlotClicked;
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<StatsChangedEvent>(OnStatsChanged);
            EventBus.Unsubscribe<EquipmentChangedEvent>(OnEquipmentChanged);
            EventBus.Unsubscribe<DialogueStartedEvent>(OnDialogueStarted);

            toggleCharacterPanelAction.action.performed -= OnTogglePerformed;
            toggleCharacterPanelAction.action.Disable();

            weaponSlot.OnSlotClicked -= OnWeaponSlotClicked;
            armorSlot.OnSlotClicked -= OnArmorSlotClicked;
            jewelrySlot.OnSlotClicked -= OnJewelrySlotClicked;

            if (characterPanelRoot.activeSelf)
            {
                GameplayInputLock.UnlockMovement(CharacterPanelLockReason);
                GameplayInputLock.UnlockCamera(CharacterPanelLockReason);
            }
        }

        private void OnStatsChanged(StatsChangedEvent evt)
        {
            if (characterPanelRoot.activeSelf)
            {
                RefreshStats();
            }
        }

        private void OnEquipmentChanged(EquipmentChangedEvent evt)
        {
            if (characterPanelRoot.activeSelf)
            {
                RefreshEquipment();
            }
        }

        private void OnTogglePerformed(InputAction.CallbackContext context)
        {
            if (characterPanelRoot.activeSelf)
            {
                ClosePanel();
                return;
            }

            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameStateType.Dialogue)
            {
                Debug.Log("Panel postaci jest niedostępny podczas rozmowy.");
                return;
            }

            OpenPanel();
        }

        private void OnDialogueStarted(DialogueStartedEvent evt)
        {
            if (characterPanelRoot.activeSelf)
            {
                ClosePanel();
            }
        }

        private void OpenPanel()
        {
            characterPanelRoot.SetActive(true);
            GameplayInputLock.LockMovement(CharacterPanelLockReason);
            GameplayInputLock.LockCamera(CharacterPanelLockReason);
            RefreshStats();
            RefreshEquipment();
        }

        private void ClosePanel()
        {
            characterPanelRoot.SetActive(false);
            GameplayInputLock.UnlockMovement(CharacterPanelLockReason);
            GameplayInputLock.UnlockCamera(CharacterPanelLockReason);
        }

        private void OnWeaponSlotClicked()
        {
            equipmentBridge.Equipment.TryUnequip(ItemType.Weapon);
        }

        private void OnArmorSlotClicked()
        {
            equipmentBridge.Equipment.TryUnequip(ItemType.Armor);
        }

        private void OnJewelrySlotClicked()
        {
            equipmentBridge.Equipment.TryUnequip(ItemType.Jewelry);
        }

        private void RefreshStats()
        {
            PlayerStatsSystem stats = playerStatsBridge.Stats;

            healthText.text = $"Zdrowie: {stats.Health.Current}/{stats.Health.Max}";
            staminaText.text = $"Kondycja: {stats.Stamina.Current}/{stats.Stamina.Max}";
            manaText.text = $"Mana: {stats.Mana.Current}/{stats.Mana.Max}";

            strengthText.text = $"Siła: {stats.Strength}";
            dexterityText.text = $"Zręczność: {stats.Dexterity}";
            enduranceText.text = $"Wytrzymałość: {stats.Endurance}";
            wisdomText.text = $"Mądrość: {stats.Wisdom}";
        }

        private void RefreshEquipment()
        {
            EquipmentSystem equipment = equipmentBridge.Equipment;

            weaponSlot.SetItem(equipment.EquippedWeapon);
            armorSlot.SetItem(equipment.EquippedArmor);
            jewelrySlot.SetItem(equipment.EquippedJewelry);
        }
    }
}
