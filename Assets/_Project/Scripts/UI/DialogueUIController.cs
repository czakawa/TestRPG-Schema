using System.Collections.Generic;
using Project.Core.Events;
using Project.Data;
using Project.Gameplay.Dialogue;
using TMPro;
using UnityEngine;

namespace Project.UI
{
    /// <summary>
    /// Panel UI rozmowy z NPC - podgląd kwestii i wybór opcji odpowiedzi. Czysto event-driven:
    /// reaguje na DialogueStartedEvent/DialogueNodeChangedEvent/DialogueEndedEvent, nie odpytuje
    /// DialogueSystem co klatkę. Ruch gracza jest już zablokowany przez stan Dialogue -
    /// ten kontroler jest wyłącznie warstwą wizualną.
    /// </summary>
    public class DialogueUIController : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private TextMeshProUGUI npcText;
        [SerializeField] private Transform optionsContainer;
        [SerializeField] private GameObject optionButtonPrefab;
        [SerializeField] private DialogueBridge dialogueBridge;

        private void OnEnable()
        {
            panelRoot.SetActive(false);

            EventBus.Subscribe<DialogueStartedEvent>(OnDialogueStarted);
            EventBus.Subscribe<DialogueNodeChangedEvent>(OnDialogueNodeChanged);
            EventBus.Subscribe<DialogueEndedEvent>(OnDialogueEnded);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<DialogueStartedEvent>(OnDialogueStarted);
            EventBus.Unsubscribe<DialogueNodeChangedEvent>(OnDialogueNodeChanged);
            EventBus.Unsubscribe<DialogueEndedEvent>(OnDialogueEnded);
        }

        private void OnDialogueStarted(DialogueStartedEvent evt)
        {
            Debug.Log("DialogueStartedEvent odebrany, aktywuję panel");
            ShowNode(evt.CurrentNode);
        }

        private void OnDialogueNodeChanged(DialogueNodeChangedEvent evt)
        {
            ShowNode(evt.CurrentNode);
        }

        private void OnDialogueEnded(DialogueEndedEvent evt)
        {
            panelRoot.SetActive(false);
            ClearOptions();
        }

        private void ShowNode(DialogueNode node)
        {
            panelRoot.SetActive(true);
            speakerNameText.text = node.SpeakerName;
            npcText.text = node.NpcText;
            Debug.Log($"Panel aktywny po SetActive: {panelRoot.activeSelf}");
            ClearOptions();

            IReadOnlyList<DialogueOption> options = node.Options;
            for (int i = 0; i < options.Count; i++)
            {
                int optionIndex = i;
                GameObject buttonInstance = Instantiate(optionButtonPrefab, optionsContainer);
                DialogueOptionButtonUI buttonUI = buttonInstance.GetComponent<DialogueOptionButtonUI>();
                buttonUI.Setup(options[i].OptionText, () => dialogueBridge.Dialogue.SelectOption(optionIndex));
            }
        }

        private void ClearOptions()
        {
            int childCount = optionsContainer.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                Destroy(optionsContainer.GetChild(i).gameObject);
            }
        }
    }
}
