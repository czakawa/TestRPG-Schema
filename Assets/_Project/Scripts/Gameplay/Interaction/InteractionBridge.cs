using Project.Core;
using Project.Gameplay.Camera;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Gameplay.Interaction
{
    /// <summary>
    /// MonoBehaviour "most" łączący czysty InteractionSystem ze światem silnika: podpina punkt
    /// pochodzenia raycasta, zasięg, warstwę interaktywnych obiektów i akcję Interact w Inspectorze,
    /// rejestruje system w GameSystemsManager przy włączeniu obiektu i wyrejestrowuje przy wyłączeniu.
    /// Przekazuje do systemu tylko delegaty odczytujące pozycję/kierunek - sam Transform nigdy
    /// nie trafia do logiki systemu.
    /// </summary>
    public class InteractionBridge : MonoBehaviour
    {
        [SerializeField] private Transform interactionOrigin;
        [SerializeField] private CameraOrbitBridge cameraOrbitBridge;
        [SerializeField] private float interactionRange = 3f;
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private InputActionReference interactAction;

        private InteractionSystem _interactionSystem;

        private void Awake()
        {
            _interactionSystem = new InteractionSystem(
                () => interactionOrigin.position,
                () => cameraOrbitBridge.CurrentLookRotation * Vector3.forward,
                interactionRange,
                interactableLayer,
                gameObject);
        }

        private void OnEnable()
        {
            interactAction.action.Enable();
            interactAction.action.performed += OnInteractPerformed;

            if (GameManager.Instance == null)
            {
                Debug.LogError("InteractionBridge: GameManager.Instance jest null - czy scena Bootstrap była uruchomiona jako pierwsza?");
                return;
            }

            GameManager.Instance.Systems.RegisterSystem(_interactionSystem);
        }

        private void OnDisable()
        {
            interactAction.action.performed -= OnInteractPerformed;
            interactAction.action.Disable();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.Systems.UnregisterSystem(_interactionSystem);
            }
        }

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            _interactionSystem.TryInteract();
        }
    }
}
