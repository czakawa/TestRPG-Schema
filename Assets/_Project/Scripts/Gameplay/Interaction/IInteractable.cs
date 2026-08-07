using UnityEngine;

namespace Project.Gameplay.Interaction
{
    /// <summary>
    /// Kontrakt dla dowolnego obiektu sceny, z którym gracz może wejść w interakcję
    /// (podniesienie przedmiotu, otwarcie drzwi, rozmowa z NPC itd.). Implementowany przez
    /// MonoBehaviour osadzone na obiektach z colliderem na warstwie "Interactable".
    /// </summary>
    public interface IInteractable
    {
        /// <summary>Tekst do wyświetlenia w UI, gdy obiekt jest namierzony przez gracza.</summary>
        string GetInteractionPrompt();

        /// <summary>Wykonuje właściwą interakcję. Wołane dopiero, gdy CanInteract zwróci true.</summary>
        void Interact(GameObject interactor);

        /// <summary>Czy interakcja jest w danym momencie możliwa (np. drzwi zamknięte na klucz).</summary>
        bool CanInteract(GameObject interactor);
    }
}
