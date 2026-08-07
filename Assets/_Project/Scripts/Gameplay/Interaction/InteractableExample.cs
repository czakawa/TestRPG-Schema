using Project.Data;
using Project.Gameplay.Inventory;
using UnityEngine;

namespace Project.Gameplay.Interaction
{
    /// <summary>
    /// Testowa implementacja IInteractable dodająca skonfigurowany przedmiot do ekwipunku gracza
    /// po interakcji. Weryfikuje pełny łańcuch InteractionSystem -> InventorySystem.
    /// </summary>
    public class InteractableExample : MonoBehaviour, IInteractable
    {
        [SerializeField] private ItemData itemToGive;
        [SerializeField] private int quantity = 1;

        public bool CanInteract(GameObject interactor)
        {
            return true;
        }

        public void Interact(GameObject interactor)
        {
            InventoryBridge inventoryBridge = interactor.GetComponentInParent<InventoryBridge>();
            if (inventoryBridge == null)
            {
                Debug.LogError("InteractableExample: brak InventoryBridge na interactorze lub jego rodzicach.");
                return;
            }

            if (inventoryBridge.Inventory.TryAddItem(itemToGive, quantity))
            {
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("Ekwipunek pełny, nie można podnieść " + itemToGive.DisplayName);
            }
        }

        public string GetInteractionPrompt()
        {
            return "Naciśnij E, aby podnieść " + itemToGive.DisplayName;
        }
    }
}
