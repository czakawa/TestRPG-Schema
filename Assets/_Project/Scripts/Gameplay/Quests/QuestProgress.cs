using System.Collections.Generic;
using Project.Data;

namespace Project.Gameplay.Quests
{
    /// <summary>
    /// Stan runtime jednego questa dla gracza. Zwykła klasa (nie ScriptableObject) - żyje
    /// wyłącznie w pamięci wewnątrz QuestSystem._activeQuests, nigdy nie jest assetem.
    /// </summary>
    public class QuestProgress
    {
        public QuestData Quest;
        public QuestState State;

        /// <summary>Równoległa tablica do Quest.Objectives - ile aktualnie zebrano/spełniono dla każdego celu.</summary>
        public int[] ObjectiveProgress;

        public QuestProgress(QuestData quest)
        {
            Quest = quest;
            State = QuestState.NotStarted;
            ObjectiveProgress = new int[quest.Objectives.Count];
        }

        /// <summary>Sprawdza czy każdy cel osiągnął swój wymagany próg (TalkToNpc traktowane jako próg 1).</summary>
        public bool AreAllObjectivesComplete()
        {
            IReadOnlyList<QuestObjective> objectives = Quest.Objectives;
            for (int i = 0; i < objectives.Count; i++)
            {
                int required = objectives[i].Type == ObjectiveType.TalkToNpc ? 1 : objectives[i].RequiredAmount;
                if (ObjectiveProgress[i] < required)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
