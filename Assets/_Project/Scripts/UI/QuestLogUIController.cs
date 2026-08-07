using System.Collections.Generic;
using Project.Core;
using Project.Core.Events;
using Project.Core.States;
using Project.Data;
using Project.Gameplay.Quests;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.UI
{
    /// <summary>
    /// Panel UI dziennika zadań - lista tytułów aktywnych/ukończonych questów po lewej,
    /// szczegóły (opis + cele z postępem) wybranego questa po prawej. Przełączany akcją
    /// ToggleQuestLog (klawisz J), odświeżany po QuestStartedEvent/QuestObjectiveUpdatedEvent/
    /// QuestCompletedEvent/QuestTurnedInEvent, dokładnie jak InventoryUIController/CharacterPanelUI.
    ///
    /// Blokuje ruch/kamerę własnym kluczem "QuestLog" w GameplayInputLock, niezależnym od
    /// "Inventory"/"CharacterPanel" - quest log może być otwarty jednocześnie z tamtymi panelami.
    /// Ten sam guard na stan Dialogue i ten sam wzorzec auto-zamknięcia po DialogueStartedEvent
    /// co InventoryUIController.
    /// </summary>
    public class QuestLogUIController : MonoBehaviour
    {
        private const string QuestLogLockReason = "QuestLog";

        [SerializeField] private QuestBridge questBridge;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Transform questListContainer;
        [SerializeField] private GameObject questListEntryPrefab;
        [SerializeField] private TextMeshProUGUI detailTitleText;
        [SerializeField] private TextMeshProUGUI detailDescriptionText;
        [SerializeField] private Transform objectivesContainer;
        [SerializeField] private GameObject objectiveEntryPrefab;
        [SerializeField] private InputActionReference toggleQuestLogAction;

        private string _selectedQuestId;

        private void Awake()
        {
            panelRoot.SetActive(false);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<QuestStartedEvent>(OnQuestStarted);
            EventBus.Subscribe<QuestObjectiveUpdatedEvent>(OnQuestObjectiveUpdated);
            EventBus.Subscribe<QuestCompletedEvent>(OnQuestCompleted);
            EventBus.Subscribe<QuestTurnedInEvent>(OnQuestTurnedIn);
            EventBus.Subscribe<DialogueStartedEvent>(OnDialogueStarted);

            toggleQuestLogAction.action.Enable();
            toggleQuestLogAction.action.performed += OnTogglePerformed;
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<QuestStartedEvent>(OnQuestStarted);
            EventBus.Unsubscribe<QuestObjectiveUpdatedEvent>(OnQuestObjectiveUpdated);
            EventBus.Unsubscribe<QuestCompletedEvent>(OnQuestCompleted);
            EventBus.Unsubscribe<QuestTurnedInEvent>(OnQuestTurnedIn);
            EventBus.Unsubscribe<DialogueStartedEvent>(OnDialogueStarted);

            toggleQuestLogAction.action.performed -= OnTogglePerformed;
            toggleQuestLogAction.action.Disable();

            if (panelRoot.activeSelf)
            {
                GameplayInputLock.UnlockMovement(QuestLogLockReason);
                GameplayInputLock.UnlockCamera(QuestLogLockReason);
            }
        }

        private void OnQuestStarted(QuestStartedEvent evt)
        {
            HandleQuestEvent(evt.Quest);
        }

        private void OnQuestObjectiveUpdated(QuestObjectiveUpdatedEvent evt)
        {
            HandleQuestEvent(evt.Quest);
        }

        private void OnQuestCompleted(QuestCompletedEvent evt)
        {
            HandleQuestEvent(evt.Quest);
        }

        private void OnQuestTurnedIn(QuestTurnedInEvent evt)
        {
            HandleQuestEvent(evt.Quest);
        }

        private void HandleQuestEvent(QuestData quest)
        {
            if (!panelRoot.activeSelf)
            {
                return;
            }

            RefreshQuestList();

            if (quest != null && quest.QuestId == _selectedQuestId)
            {
                RefreshDetailPanel();
            }
        }

        private void OnTogglePerformed(InputAction.CallbackContext context)
        {
            if (panelRoot.activeSelf)
            {
                ClosePanel();
                return;
            }

            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameStateType.Dialogue)
            {
                Debug.Log("Dziennik zadań jest niedostępny podczas rozmowy.");
                return;
            }

            OpenPanel();
        }

        private void OnDialogueStarted(DialogueStartedEvent evt)
        {
            if (panelRoot.activeSelf)
            {
                ClosePanel();
            }
        }

        private void OpenPanel()
        {
            panelRoot.SetActive(true);
            GameplayInputLock.LockMovement(QuestLogLockReason);
            GameplayInputLock.LockCamera(QuestLogLockReason);
            RefreshQuestList();
            RefreshDetailPanel();
        }

        private void ClosePanel()
        {
            panelRoot.SetActive(false);
            GameplayInputLock.UnlockMovement(QuestLogLockReason);
            GameplayInputLock.UnlockCamera(QuestLogLockReason);
        }

        private void RefreshQuestList()
        {
            int childCount = questListContainer.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                Destroy(questListContainer.GetChild(i).gameObject);
            }

            foreach (KeyValuePair<string, QuestProgress> kvp in questBridge.Quest.ActiveQuests)
            {
                QuestProgress progress = kvp.Value;
                if (progress.State != QuestState.Active && progress.State != QuestState.Completed)
                {
                    continue;
                }

                GameObject entryInstance = Instantiate(questListEntryPrefab, questListContainer);
                QuestListEntryUI entryUI = entryInstance.GetComponent<QuestListEntryUI>();

                string questId = progress.Quest.QuestId;
                entryUI.Setup(progress.Quest.Title, () => OnQuestEntryClicked(questId));
            }
        }

        private void OnQuestEntryClicked(string questId)
        {
            _selectedQuestId = questId;
            RefreshDetailPanel();
        }

        private void RefreshDetailPanel()
        {
            int childCount = objectivesContainer.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                Destroy(objectivesContainer.GetChild(i).gameObject);
            }

            QuestProgress progress = GetSelectedProgress();
            if (progress == null)
            {
                detailTitleText.text = string.Empty;
                detailDescriptionText.text = string.Empty;
                return;
            }

            detailTitleText.text = progress.Quest.Title;
            detailDescriptionText.text = progress.Quest.Description;

            IReadOnlyList<QuestObjective> objectives = progress.Quest.Objectives;
            for (int i = 0; i < objectives.Count; i++)
            {
                QuestObjective objective = objectives[i];
                int progressAmount = progress.ObjectiveProgress[i];

                string line;
                if (objective.Type == ObjectiveType.TalkToNpc)
                {
                    line = $"{objective.Description} ({(progressAmount >= 1 ? "Wykonano" : "Nie wykonano")})";
                }
                else
                {
                    line = $"{objective.Description} ({progressAmount}/{objective.RequiredAmount})";
                }

                GameObject entryInstance = Instantiate(objectiveEntryPrefab, objectivesContainer);
                QuestObjectiveEntryUI entryUI = entryInstance.GetComponent<QuestObjectiveEntryUI>();
                entryUI.Setup(line);
            }
        }

        /// <summary>Zwraca stan wybranego questa tylko jeśli wciąż jest Active/Completed - questa właśnie oddanego (TurnedIn) traktujemy jak nieistniejący, tak samo jak brak wyboru.</summary>
        private QuestProgress GetSelectedProgress()
        {
            if (_selectedQuestId == null)
            {
                return null;
            }

            if (!questBridge.Quest.ActiveQuests.TryGetValue(_selectedQuestId, out QuestProgress progress))
            {
                return null;
            }

            if (progress.State != QuestState.Active && progress.State != QuestState.Completed)
            {
                return null;
            }

            return progress;
        }
    }
}
