using System.Collections.Generic;
using Project.Core.Events;
using Project.Core.Systems;
using Project.Data;
using Project.Gameplay.Economy;
using Project.Gameplay.Inventory;
using UnityEngine;

namespace Project.Gameplay.Quests
{
    /// <summary>
    /// Czysta klasa C# (nie MonoBehaviour) zarządzająca questami gracza. Event-driven, nie
    /// pollingowy - ReportItemCollected/ReportNpcTalkedTo są wołane z zewnątrz (QuestBridge,
    /// NpcDialogueInteractable) w reakcji na zmiany w innych systemach, dlatego Tick jest pusty.
    /// Wymaga referencji do InventorySystem (sprawdzanie/wypłata CollectItem) i CurrencySystem
    /// (wypłata złota z nagrody).
    /// </summary>
    public class QuestSystem : IGameSystem
    {
        private readonly InventorySystem _inventorySystem;
        private readonly CurrencySystem _currencySystem;
        private readonly Dictionary<string, QuestProgress> _activeQuests = new Dictionary<string, QuestProgress>();

        /// <summary>Wszystkie questy, które gracz kiedykolwiek rozpoczął (dowolny stan poza NotStarted) - do UI quest logu.</summary>
        public IReadOnlyDictionary<string, QuestProgress> ActiveQuests => _activeQuests;

        public QuestSystem(InventorySystem inventorySystem, CurrencySystem currencySystem)
        {
            _inventorySystem = inventorySystem;
            _currencySystem = currencySystem;
        }

        public void Initialize()
        {
        }

        public void Tick(float deltaTime)
        {
        }

        public void FixedTick(float fixedDeltaTime)
        {
        }

        public void Shutdown()
        {
        }

        /// <summary>Rozpoczyna quest. Zwraca false, jeśli quest już istnieje w słowniku (niezależnie od stanu) - nie da się zacząć drugi raz.</summary>
        public bool StartQuest(QuestData quest)
        {
            if (quest == null || _activeQuests.ContainsKey(quest.QuestId))
            {
                return false;
            }

            QuestProgress progress = new QuestProgress(quest);
            progress.State = QuestState.Active;
            _activeQuests[quest.QuestId] = progress;

            EventBus.Publish(new QuestStartedEvent(quest));
            return true;
        }

        /// <summary>
        /// Wywoływane z zewnątrz przy każdej zmianie ekwipunku (patrz QuestBridge). Dla każdego
        /// aktywnego questa przelicza cele CollectItem o pasującym targetId na nowo z faktycznego
        /// stanu ekwipunku (nie inkrementuje ślepo - gracz mógł też wyrzucić/zużyć przedmiot).
        /// </summary>
        public void ReportItemCollected(string itemId, int amount)
        {
            foreach (QuestProgress progress in _activeQuests.Values)
            {
                if (progress.State != QuestState.Active)
                {
                    continue;
                }

                bool matchedAny = false;
                IReadOnlyList<QuestObjective> objectives = progress.Quest.Objectives;
                for (int i = 0; i < objectives.Count; i++)
                {
                    QuestObjective objective = objectives[i];
                    if (objective.Type != ObjectiveType.CollectItem || objective.TargetId != itemId)
                    {
                        continue;
                    }

                    matchedAny = true;
                    progress.ObjectiveProgress[i] = Mathf.Min(_inventorySystem.GetItemCount(itemId), objective.RequiredAmount);
                }

                if (matchedAny)
                {
                    CompleteIfReady(progress);
                }
            }
        }

        /// <summary>Wywoływane bezpośrednio z NpcDialogueInteractable po rozpoczęciu rozmowy z danym NPC.</summary>
        public void ReportNpcTalkedTo(string npcDialogueId)
        {
            foreach (QuestProgress progress in _activeQuests.Values)
            {
                if (progress.State != QuestState.Active)
                {
                    continue;
                }

                bool matchedAny = false;
                IReadOnlyList<QuestObjective> objectives = progress.Quest.Objectives;
                for (int i = 0; i < objectives.Count; i++)
                {
                    QuestObjective objective = objectives[i];
                    if (objective.Type != ObjectiveType.TalkToNpc || objective.TargetId != npcDialogueId)
                    {
                        continue;
                    }

                    matchedAny = true;
                    progress.ObjectiveProgress[i] = 1;
                }

                if (matchedAny)
                {
                    CompleteIfReady(progress);
                }
            }
        }

        /// <summary>
        /// Próbuje oddać ukończony quest i odebrać nagrodę. All-or-nothing: jeśli nagroda zawiera
        /// przedmiot i nie ma na niego miejsca w ekwipunku, operacja jest przerywana - złoto nie
        /// jest wypłacane, a stan questa się nie zmienia.
        /// </summary>
        public bool TryTurnInQuest(string questId)
        {
            if (!_activeQuests.TryGetValue(questId, out QuestProgress progress) || progress.State != QuestState.Completed)
            {
                return false;
            }

            QuestReward reward = progress.Quest.Reward;
            if (reward.RewardItem != null && !_inventorySystem.TryAddItem(reward.RewardItem, reward.RewardItemQuantity))
            {
                return false;
            }

            if (reward.GoldAmount > 0)
            {
                _currencySystem.AddGold(reward.GoldAmount);
            }

            progress.State = QuestState.TurnedIn;
            EventBus.Publish(new QuestTurnedInEvent(progress.Quest));
            return true;
        }

        private void CompleteIfReady(QuestProgress progress)
        {
            EventBus.Publish(new QuestObjectiveUpdatedEvent(progress.Quest));

            if (progress.State == QuestState.Active && progress.AreAllObjectivesComplete())
            {
                progress.State = QuestState.Completed;
                EventBus.Publish(new QuestCompletedEvent(progress.Quest));
            }
        }
    }
}
