using Project.Core.States;

namespace Project.Core.Events
{
    /// <summary>
    /// Publikowane przez <see cref="GameStateMachine"/> zaraz po zakończeniu tranzycji między stanami.
    /// Pozwala np. warstwie UI reagować na zmianę stanu bez bezpośredniej referencji do state machine.
    /// </summary>
    public readonly struct GameStateChangedEvent
    {
        public readonly GameStateType PreviousState;
        public readonly GameStateType NewState;

        public GameStateChangedEvent(GameStateType previousState, GameStateType newState)
        {
            PreviousState = previousState;
            NewState = newState;
        }
    }

    /// <summary>
    /// Publikowane przez <see cref="Project.Gameplay.Interaction.InteractionSystem"/>, gdy gracz spojrzy
    /// na nowy obiekt implementujący IInteractable. UI (np. prompt "Naciśnij E") subskrybuje to zdarzenie
    /// zamiast trzymać referencję do systemu interakcji.
    /// </summary>
    public readonly struct InteractableFocusedEvent
    {
        public readonly string Prompt;

        public InteractableFocusedEvent(string prompt)
        {
            Prompt = prompt;
        }
    }

    /// <summary>
    /// Publikowane przez <see cref="Project.Gameplay.Interaction.InteractionSystem"/>, gdy żaden obiekt
    /// interaktywny nie jest już w zasięgu/polu widzenia gracza. Pusty struct pełni rolę sygnału.
    /// </summary>
    public readonly struct InteractableFocusLostEvent
    {
    }

    /// <summary>
    /// Publikowane przez <see cref="Project.Gameplay.Inventory.InventorySystem"/> po każdej udanej zmianie
    /// zawartości ekwipunku. Pusty sygnał - subskrybenci (UI, debug logger) odczytują aktualny stan
    /// przez referencję do Slots, nie przez dane w evencie.
    /// </summary>
    public readonly struct InventoryChangedEvent
    {
    }
}
