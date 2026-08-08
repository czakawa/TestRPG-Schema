using Project.Core.States;
using Project.Data;
using UnityEngine;

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

    /// <summary>
    /// Publikowane przez <see cref="Project.Gameplay.Stats.PlayerStatsSystem"/> po każdej zmianie
    /// Vitali (ModifyHealth/ModifyStamina/ModifyMana) lub atrybutu (SetAttribute). Pusty sygnał -
    /// subskrybenci (CharacterPanelUI) odczytują aktualny stan przez referencję do systemu,
    /// tak samo jak InventoryChangedEvent.
    /// </summary>
    public readonly struct StatsChangedEvent
    {
    }

    /// <summary>
    /// Publikowane przez <see cref="Project.Gameplay.Equipment.EquipmentSystem"/> po każdej udanej
    /// zmianie założonego ekwipunku (TryEquip/TryUnequip). Pusty sygnał - subskrybenci
    /// (CharacterPanelUI) odczytują aktualny stan przez referencję do systemu.
    /// </summary>
    public readonly struct EquipmentChangedEvent
    {
    }

    /// <summary>
    /// Publikowane przez <see cref="Project.Gameplay.Quests.QuestSystem"/>, gdy quest zostaje
    /// rozpoczęty (StartQuest).
    /// </summary>
    public readonly struct QuestStartedEvent
    {
        public readonly QuestData Quest;

        public QuestStartedEvent(QuestData quest)
        {
            Quest = quest;
        }
    }

    /// <summary>
    /// Publikowane przez <see cref="Project.Gameplay.Quests.QuestSystem"/> po każdej aktualizacji
    /// postępu celu aktywnego questa (ReportItemCollected/ReportNpcTalkedTo).
    /// </summary>
    public readonly struct QuestObjectiveUpdatedEvent
    {
        public readonly QuestData Quest;

        public QuestObjectiveUpdatedEvent(QuestData quest)
        {
            Quest = quest;
        }
    }

    /// <summary>
    /// Publikowane przez <see cref="Project.Gameplay.Quests.QuestSystem"/>, gdy wszystkie cele
    /// questa zostają spełnione (stan Active -&gt; Completed).
    /// </summary>
    public readonly struct QuestCompletedEvent
    {
        public readonly QuestData Quest;

        public QuestCompletedEvent(QuestData quest)
        {
            Quest = quest;
        }
    }

    /// <summary>
    /// Publikowane przez <see cref="Project.Gameplay.Quests.QuestSystem"/> po udanym odebraniu
    /// nagrody (TryTurnInQuest, stan Completed -&gt; TurnedIn).
    /// </summary>
    public readonly struct QuestTurnedInEvent
    {
        public readonly QuestData Quest;

        public QuestTurnedInEvent(QuestData quest)
        {
            Quest = quest;
        }
    }

    /// <summary>
    /// Publikowane przez <see cref="Project.Gameplay.Dialogue.DialogueSystem"/> w SelectOption, gdy
    /// wybrana opcja niesie akcję questową (QuestActionType != None), PRZED przejściem do kolejnego
    /// węzła lub zakończeniem rozmowy. DialogueSystem nie zależy bezpośrednio od QuestSystem -
    /// QuestBridge subskrybuje ten event i tłumaczy go na StartQuest/TryTurnInQuest, zachowując
    /// komunikację między systemami wyłącznie przez EventBus.
    /// </summary>
    public readonly struct DialogueQuestActionEvent
    {
        public readonly QuestActionType Action;
        public readonly QuestData Quest;

        public DialogueQuestActionEvent(QuestActionType action, QuestData quest)
        {
            Action = action;
            Quest = quest;
        }
    }

    /// <summary>
    /// Publikowane przez <see cref="Project.Gameplay.Combat.CombatSystem"/> po trafieniu ataku gracza
    /// w obiekt implementujący IDamageable. Niesie referencję do trafionego GameObject, nie do
    /// ScriptableObject jak pozostałe eventy - to świadomy wyjątek: celem ataku jest zawsze obiekt
    /// sceny (wróg), a nie dana konfiguracyjna w rodzaju ItemData/QuestData.
    /// </summary>
    public readonly struct EnemyDamagedEvent
    {
        public readonly GameObject Target;
        public readonly float Damage;

        public EnemyDamagedEvent(GameObject target, float damage)
        {
            Target = target;
            Damage = damage;
        }
    }

    /// <summary>
    /// Publikowane przez <see cref="Project.Gameplay.Combat.EnemyController"/>, gdy HP wroga spadnie
    /// do zera (TakeDamage). Wróg jest niszczony z małym opóźnieniem po publikacji tego eventu, żeby
    /// subskrybenci (przyszły loot/questy) zdążyli odczytać jego stan w tej samej klatce.
    /// </summary>
    public readonly struct EnemyDiedEvent
    {
        public readonly GameObject Enemy;

        public EnemyDiedEvent(GameObject enemy)
        {
            Enemy = enemy;
        }
    }

    /// <summary>
    /// Publikowane przez <see cref="Project.Gameplay.Stats.PlayerStatsSystem"/>, gdy Health.Current
    /// spadnie do zera (ModifyHealth), dokładnie raz na przejście z >0 na 0. Pusty sygnał -
    /// subskrybenci (PlayerDeathHandler zmieniający stan gry na GameOver, GameOverUIController
    /// pokazujący panel) nie potrzebują żadnych danych z eventu, tylko samego faktu śmierci.
    /// </summary>
    public readonly struct PlayerDiedEvent
    {
    }

    /// <summary>
    /// Publikowane przez <see cref="Project.Gameplay.Dialogue.DialogueSystem"/> w SelectOption, gdy
    /// wybrana opcja ma OpensTrade == true, PRZED sprawdzeniem NextNodeIndex - analogicznie do
    /// DialogueQuestActionEvent. Pusty sygnał: DialogueSystem nie wie nic o NpcMerchant/handlu,
    /// więc nie mógłby i tak przekazać referencji do właściwego NPC. Odbiorca (TradeUIController)
    /// odnajduje właściwego kupca przez <see cref="Project.Gameplay.Dialogue.NpcDialogueInteractable.ActiveSpeaker"/>,
    /// nie przez dane w tym evencie.
    /// </summary>
    public readonly struct TradeRequestedEvent
    {
    }

    /// <summary>
    /// Publikowane przez <see cref="Project.Gameplay.Trade.NpcMerchant"/> po każdej udanej transakcji
    /// (TryBuy/TrySell). Pusty sygnał - subskrybenci (TradeUIController) odczytują aktualny stan
    /// przez referencję do NpcMerchant.Stock, tak samo jak InventoryChangedEvent/EquipmentChangedEvent.
    /// </summary>
    public readonly struct TradeStockChangedEvent
    {
    }
}
