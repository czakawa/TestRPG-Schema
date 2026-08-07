using Project.Core;
using Project.Core.Events;
using Project.Core.States;
using Project.Data;
using Project.Gameplay.Interaction;
using UnityEngine;

namespace Project.Gameplay.Dialogue
{
    /// <summary>
    /// IInteractable rozpoczynający rozmowę z NPC: przełącza grę w stan Dialogue (blokując ruch/
    /// interakcję poza samą rozmową) i startuje DialogueSystem. Wraca do stanu Gameplay po
    /// DialogueEndedEvent - nie czeka na jawne zamknięcie okna dialogowego przez gracza.
    /// </summary>
    public class NpcDialogueInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private DialogueTree dialogueTree;

        private void OnEnable()
        {
            EventBus.Subscribe<DialogueEndedEvent>(OnDialogueEnded);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<DialogueEndedEvent>(OnDialogueEnded);
        }

        public bool CanInteract(GameObject interactor)
        {
            DialogueBridge dialogueBridge = interactor.GetComponentInParent<DialogueBridge>();
            return dialogueBridge != null && !dialogueBridge.Dialogue.IsActive;
        }

        public void Interact(GameObject interactor)
        {
            DialogueBridge dialogueBridge = interactor.GetComponentInParent<DialogueBridge>();
            if (dialogueBridge == null)
            {
                Debug.LogError("NpcDialogueInteractable: brak DialogueBridge na interactorze lub jego rodzicach.");
                return;
            }

            GameManager.Instance.ChangeState(GameStateType.Dialogue);
            dialogueBridge.Dialogue.StartDialogue(dialogueTree);
        }

        public string GetInteractionPrompt()
        {
            return "Naciśnij E, aby porozmawiać";
        }

        private void OnDialogueEnded(DialogueEndedEvent evt)
        {
            GameManager.Instance.ChangeState(GameStateType.Gameplay);
        }
    }
}
