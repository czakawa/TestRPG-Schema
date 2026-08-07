using Project.Core;
using Project.Gameplay.Inventory;
using UnityEngine;

namespace Project.Gameplay.Equipment
{
    /// <summary>
    /// MonoBehaviour "most" łączący czysty EquipmentSystem ze światem silnika. Wymaga referencji
    /// do InventoryBridge w Inspectorze - EquipmentSystem operuje na tym samym InventorySystem,
    /// z którego zdejmowane/do którego zwracane są zakładane przedmioty. System jest tworzony
    /// w OnEnable (nie w Awake, jak CurrencyBridge/InventoryBridge) celowo: Unity nie gwarantuje
    /// kolejności Awake między różnymi komponentami na tym samym GameObjekcie (Player), a
    /// inventoryBridge.Inventory musi już istnieć w momencie budowy EquipmentSystem. OnEnable
    /// jest bezpieczne, bo Unity wywołuje Awake na wszystkich obiektach sceny przed OnEnable
    /// na którymkolwiek z nich.
    /// </summary>
    public class EquipmentBridge : MonoBehaviour
    {
        [SerializeField] private InventoryBridge inventoryBridge;

        private EquipmentSystem _equipmentSystem;

        /// <summary>Referencja do systemu ekwipunku zakładanego - do użytku przez UI (CharacterPanelUI).</summary>
        public EquipmentSystem Equipment => _equipmentSystem;

        private void OnEnable()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("EquipmentBridge: GameManager.Instance jest null - czy scena Bootstrap była uruchomiona jako pierwsza?");
                return;
            }

            if (inventoryBridge == null)
            {
                Debug.LogError("EquipmentBridge: brak referencji do InventoryBridge w Inspectorze.");
                return;
            }

            _equipmentSystem = new EquipmentSystem(inventoryBridge.Inventory);
            GameManager.Instance.Systems.RegisterSystem(_equipmentSystem);
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null && _equipmentSystem != null)
            {
                GameManager.Instance.Systems.UnregisterSystem(_equipmentSystem);
            }
        }
    }
}
