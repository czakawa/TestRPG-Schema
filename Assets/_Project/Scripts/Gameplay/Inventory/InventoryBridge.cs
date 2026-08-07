using Project.Core;
using UnityEngine;

namespace Project.Gameplay.Inventory
{
    /// <summary>
    /// MonoBehaviour "most" łączący czysty InventorySystem ze światem silnika: rejestruje system
    /// w GameSystemsManager przy włączeniu obiektu i wyrejestrowuje przy wyłączeniu. Wystawia
    /// referencję do InventorySystem, żeby inne mosty (np. InteractableExample) mogły dodawać
    /// przedmioty bez znajomości wewnętrznej struktury ekwipunku.
    /// </summary>
    public class InventoryBridge : MonoBehaviour
    {
        [SerializeField] private int capacity = 20;

        private InventorySystem _inventorySystem;

        /// <summary>Referencja do systemu ekwipunku - do użytku przez inne mosty (np. InteractableExample).</summary>
        public InventorySystem Inventory => _inventorySystem;

        private void Awake()
        {
            _inventorySystem = new InventorySystem(capacity);
        }

        private void OnEnable()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("InventoryBridge: GameManager.Instance jest null - czy scena Bootstrap była uruchomiona jako pierwsza?");
                return;
            }

            GameManager.Instance.Systems.RegisterSystem(_inventorySystem);
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.Systems.UnregisterSystem(_inventorySystem);
            }
        }
    }
}
