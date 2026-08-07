using Project.Core;
using Project.Core.Events;
using UnityEngine;

namespace Project.Gameplay.Dialogue
{
    /// <summary>
    /// MonoBehaviour "most" łączący czysty DialogueSystem ze światem silnika: rejestruje system
    /// w GameSystemsManager przy włączeniu obiektu i wyrejestrowuje przy wyłączeniu. Wystawia
    /// referencję do DialogueSystem, żeby NPC (np. NpcDialogueInteractable) i przyszłe UI
    /// mogły sterować rozmową. Dodatkowo blokuje/odblokowuje kamerę przez GameplayInputLock -
    /// CameraOrbitBridge czyta input w Update(), poza pętlą GameSystemsManager, więc samo
    /// SetSystemsActive(false) ze stanu Dialogue tego nie załatwia.
    /// </summary>
    public class DialogueBridge : MonoBehaviour
    {
        private const string CameraLockReason = "Dialogue";

        private DialogueSystem _dialogueSystem;

        /// <summary>Referencja do systemu dialogu - do użytku przez NPC i UI.</summary>
        public DialogueSystem Dialogue => _dialogueSystem;

        private void Awake()
        {
            _dialogueSystem = new DialogueSystem();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<DialogueStartedEvent>(OnDialogueStarted);
            EventBus.Subscribe<DialogueEndedEvent>(OnDialogueEnded);

            if (GameManager.Instance == null)
            {
                Debug.LogError("DialogueBridge: GameManager.Instance jest null - czy scena Bootstrap była uruchomiona jako pierwsza?");
                return;
            }

            GameManager.Instance.Systems.RegisterSystem(_dialogueSystem);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<DialogueStartedEvent>(OnDialogueStarted);
            EventBus.Unsubscribe<DialogueEndedEvent>(OnDialogueEnded);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.Systems.UnregisterSystem(_dialogueSystem);
            }
        }

        private void OnDialogueStarted(DialogueStartedEvent evt)
        {
            GameplayInputLock.LockCamera(CameraLockReason);
        }

        private void OnDialogueEnded(DialogueEndedEvent evt)
        {
            GameplayInputLock.UnlockCamera(CameraLockReason);
        }
    }
}
