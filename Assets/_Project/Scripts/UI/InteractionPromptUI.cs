using Project.Core;
using Project.Core.Events;
using TMPro;
using UnityEngine;

namespace Project.UI
{
    /// <summary>
    /// Prosty, czysto event-driven HUD prompt ("Naciśnij E, żeby...") - bez Update(), więc nie dotyczy
    /// go pułapka self-disable (ta dotyczy tylko komponentów z Update() wyłączających własny obiekt).
    /// panelRoot mimo to jest osobnym dzieckiem zgodnie z konwencją reszty UI w projekcie.
    /// </summary>
    public class InteractionPromptUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TextMeshProUGUI promptText;

        private bool _isFocused;

        private void Awake()
        {
            panelRoot.SetActive(false);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<InteractableFocusedEvent>(OnInteractableFocused);
            EventBus.Subscribe<InteractableFocusLostEvent>(OnInteractableFocusLost);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<InteractableFocusedEvent>(OnInteractableFocused);
            EventBus.Unsubscribe<InteractableFocusLostEvent>(OnInteractableFocusLost);
        }

        private void OnInteractableFocused(InteractableFocusedEvent evt)
        {
            promptText.text = evt.Prompt;
            _isFocused = true;
            RefreshVisibility();
        }

        private void OnInteractableFocusLost(InteractableFocusLostEvent evt)
        {
            _isFocused = false;
            RefreshVisibility();
        }

        private void Update()
        {
            if (_isFocused)
            {
                RefreshVisibility();
            }
        }

        private void RefreshVisibility()
        {
            panelRoot.SetActive(_isFocused && !GameplayInputLock.IsCameraLocked);
        }
    }
}
