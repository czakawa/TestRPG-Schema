using System.Collections.Generic;
using UnityEngine;

namespace Project.Data
{
    /// <summary>
    /// Statyczna definicja questa. Instancje tworzone jako assety przez menu Gothic/Quest,
    /// referencjonowane z QuestSystem/DialogueOption, nigdy nie modyfikowane w runtime
    /// (stan questa dla gracza trzyma <see cref="Project.Gameplay.Quests.QuestProgress"/>).
    /// </summary>
    [CreateAssetMenu(fileName = "NewQuest", menuName = "Gothic/Quest")]
    public class QuestData : ScriptableObject
    {
        [SerializeField] private string questId;
        [SerializeField] private string title;
        [TextArea]
        [SerializeField] private string description;
        [SerializeField] private List<QuestObjective> objectives = new List<QuestObjective>();
        [SerializeField] private QuestReward reward;

        /// <summary>Unikalny identyfikator questa (np. "collect_iron_ore") - niezależny od nazwy assetu.</summary>
        public string QuestId => questId;

        public string Title => title;

        public string Description => description;

        public IReadOnlyList<QuestObjective> Objectives => objectives;

        public QuestReward Reward => reward;
    }
}
