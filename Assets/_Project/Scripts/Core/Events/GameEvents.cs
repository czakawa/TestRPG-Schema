using Project.Core.States;
using Project.Data;

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

    /// <summary>
    /// Publikowane przez <see cref="Project.Gameplay.Economy.CurrencySystem"/> po każdej udanej zmianie
    /// ilości złota (AddGold/TrySpendGold). Niesie już wyliczoną nową wartość, żeby prosty HUD
    /// (np. CurrencyDisplayUI) nie musiał trzymać referencji do systemu tylko po to, by ją odczytać.
    /// </summary>
    public readonly struct CurrencyChangedEvent
    {
        public readonly int NewAmount;

        public CurrencyChangedEvent(int newAmount)
        {
            NewAmount = newAmount;
        }
    }

    /// <summary>
    /// Publikowane przez <see cref="Project.Gameplay.Dialogue.DialogueSystem"/> przy rozpoczęciu rozmowy
    /// (węzeł startNodeIndex danego drzewa).
    /// </summary>
    public readonly struct DialogueStartedEvent
    {
        public readonly DialogueNode CurrentNode;

        public DialogueStartedEvent(DialogueNode currentNode)
        {
            CurrentNode = currentNode;
        }
    }

    /// <summary>
    /// Publikowane przez <see cref="Project.Gameplay.Dialogue.DialogueSystem"/> po wyborze opcji
    /// prowadzącej do kolejnego węzła (nextNodeIndex != -1).
    /// </summary>
    public readonly struct DialogueNodeChangedEvent
    {
        public readonly DialogueNode CurrentNode;

        public DialogueNodeChangedEvent(DialogueNode currentNode)
        {
            CurrentNode = currentNode;
        }
    }

    /// <summary>
    /// Publikowane przez <see cref="Project.Gameplay.Dialogue.DialogueSystem"/> po zakończeniu rozmowy
    /// (wybór opcji z nextNodeIndex == -1 lub bezpośrednie wywołanie EndDialogue). Pusty sygnał -
    /// subskrybenci (np. NpcDialogueInteractable) reagują wracając do stanu Gameplay.
    /// </summary>
    public readonly struct DialogueEndedEvent
    {
    }
}
