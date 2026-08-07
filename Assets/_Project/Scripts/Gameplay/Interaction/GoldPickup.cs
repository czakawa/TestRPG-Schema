using Project.Gameplay.Economy;
using UnityEngine;

namespace Project.Gameplay.Interaction
{
    /// <summary>
    /// IInteractable dodający złoto po interakcji. Osobna klasa od InteractableExample - semantyka
    /// jest inna (waluta, nie przedmiot ekwipunku), więc nie trafia do InventorySystem.
    /// </summary>
    public class GoldPickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private int goldAmount = 1;

        public bool CanInteract(GameObject interactor)
        {
            return true;
        }

        public void Interact(GameObject interactor)
        {
            CurrencyBridge currencyBridge = interactor.GetComponentInParent<CurrencyBridge>();
            if (currencyBridge == null)
            {
                Debug.LogError("GoldPickup: brak CurrencyBridge na interactorze lub jego rodzicach.");
                return;
            }

            currencyBridge.Currency.AddGold(goldAmount);
            Destroy(gameObject);
        }

        public string GetInteractionPrompt()
        {
            return "Naciśnij E, aby podnieść " + goldAmount + " złota";
        }
    }
}
