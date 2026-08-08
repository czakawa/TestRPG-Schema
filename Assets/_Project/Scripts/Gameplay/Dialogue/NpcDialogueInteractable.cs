using Project.Core;
using Project.Core.Events;
using Project.Core.States;
using Project.Data;
using Project.Gameplay.Interaction;
using Project.Gameplay.Quests;
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

        /// <summary>
        /// GameObject NPC aktualnie prowadzącego rozmowę z graczem - jedyne miejsce w projekcie,
        /// które wie, KTÓRY konkretny NPC odpowiada za aktywny DialogueSystem (sam DialogueSystem
        /// zna tylko DialogueTree/węzły, nie obiekt sceny, który je uruchomił). Istnieje głównie dla
        /// TradeRequestedEvent: ten event jest pustym sygnałem (DialogueSystem nie zna NpcMerchant),
        /// więc TradeUIController odnajduje właściwego kupca przez ActiveSpeaker.GetComponent&lt;NpcMerchant&gt;()
        /// zamiast przez dane w evencie. Ustawiane synchronicznie w Interact() - zawsze na długo PRZED
        /// jakimkolwiek TradeRequestedEvent (ten może powstać dopiero z późniejszego SelectOption
        /// w already-aktywnej rozmowie), więc nie ma tu hazardu kolejności między subskrybentami tego
        /// samego eventu. Czyszczone po DialogueEndedEvent - w danym momencie może być aktywna tylko
        /// jedna rozmowa (CanInteract już to wymusza), więc statyczne pole jest bezpieczne.
        /// </summary>
        public static GameObject ActiveSpeaker { get; private set; }

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

            ActiveSpeaker = gameObject;
            GameManager.Instance.ChangeState(GameStateType.Dialogue);
            dialogueBridge.Dialogue.StartDialogue(dialogueTree);

            QuestBridge questBridge = interactor.GetComponentInParent<QuestBridge>();
            if (questBridge != null)
            {
                questBridge.Quest.ReportNpcTalkedTo(dialogueTree.DialogueId);
            }
        }

        public string GetInteractionPrompt()
        {
            return "Naciśnij E, aby porozmawiać";
        }

        private void OnDialogueEnded(DialogueEndedEvent evt)
        {
            GameManager.Instance.ChangeState(GameStateType.Gameplay);

            if (ActiveSpeaker == gameObject)
            {
                ActiveSpeaker = null;
            }
        }
    }
}
