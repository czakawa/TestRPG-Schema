using System;
using Project.Gameplay.Stats;
using UnityEngine;

namespace Project.Data
{
    /// <summary>
    /// Akcja questowa powiązana z opcją dialogową. DialogueSystem.SelectOption publikuje
    /// DialogueQuestActionEvent z tą wartością - QuestBridge subskrybuje event i tłumaczy go
    /// na QuestSystem.StartQuest/TryTurnInQuest.
    /// </summary>
    public enum QuestActionType
    {
        None,
        StartQuest,
        TurnInQuest
    }

    /// <summary>
    /// Pojedyncza opcja odpowiedzi gracza w węźle dialogowym. Zwykła serializowalna klasa
    /// (nie ScriptableObject) - zagnieżdżona w liście wewnątrz DialogueNode.
    /// </summary>
    [Serializable]
    public class DialogueOption
    {
        [SerializeField] private string optionText;
        [SerializeField] private int nextNodeIndex = -1;
        [SerializeField] private QuestActionType questAction = QuestActionType.None;
        [SerializeField] private QuestData targetQuest;
        [SerializeField] private bool opensTrade;
        [SerializeField] private bool triggersTraining;
        [SerializeField] private AttributeType trainingAttribute;

        public string OptionText => optionText;

        /// <summary>Indeks w DialogueTree.Nodes, do którego przechodzi rozmowa. -1 = koniec rozmowy.</summary>
        public int NextNodeIndex => nextNodeIndex;

        /// <summary>Akcja questowa wykonywana po wybraniu tej opcji, przed przejściem do kolejnego węzła.</summary>
        public QuestActionType QuestAction => questAction;

        /// <summary>Quest, którego dotyczy QuestAction. Ignorowane, jeśli QuestAction == None.</summary>
        public QuestData TargetQuest => targetQuest;

        /// <summary>Czy wybranie tej opcji publikuje TradeRequestedEvent (otwiera panel handlu z tym NPC).
        /// Niezależne od QuestAction/NextNodeIndex - może współistnieć z akcją questową na tej samej
        /// opcji, i typowo kończy rozmowę (NextNodeIndex == -1) w tym samym wyborze.</summary>
        public bool OpensTrade => opensTrade;

        /// <summary>Czy wybranie tej opcji publikuje TrainingRequestedEvent - natychmiastowa transakcja
        /// nauki, bez osobnego UI, analogicznie do OpensTrade ale bez panelu.</summary>
        public bool TriggersTraining => triggersTraining;
        /// <summary>Atrybut do wytrenowania. Ignorowane, jeśli TriggersTraining == false.</summary>
        public AttributeType TrainingAttribute => trainingAttribute;
    }
}
