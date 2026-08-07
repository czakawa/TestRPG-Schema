using Project.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Gameplay.Camera
{
    /// <summary>
    /// MonoBehaviour "most" łączący czysty CameraOrbitSystem ze światem silnika: podpina CameraAnchor,
    /// Main Camera i akcję Look w Inspectorze, rejestruje system w GameSystemsManager przy włączeniu
    /// obiektu i wyrejestrowuje przy wyłączeniu. Co klatkę obraca CameraAnchor wg wyliczonego stanu,
    /// a następnie teleportuje kamerę (niebędącą dzieckiem Playera) do jej pozycji i rotacji.
    /// </summary>
    public class CameraOrbitBridge : MonoBehaviour
    {
        [SerializeField] private Transform cameraAnchor;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private InputActionReference lookAction;

        [Header("Tuning")]
        [SerializeField] private float sensitivity = 0.1f;
        [SerializeField] private float pitchMin = -20f;
        [SerializeField] private float pitchMax = 60f;
        [SerializeField] private float recenterDelay = 1.5f;
        [SerializeField] private float recenterSpeed = 3f;

        private CameraOrbitSystem _orbitSystem;

        /// <summary>Bieżąca rotacja orbitalna, świeża co klatkę (bez opóźnienia LateUpdate) - do użytku
        /// przez systemy potrzebujące kierunku patrzenia kamery przed jej fizycznym zaktualizowaniem, np. InteractionBridge.</summary>
        public Quaternion CurrentLookRotation => _orbitSystem.CurrentRotation;

        private void Awake()
        {
            _orbitSystem = new CameraOrbitSystem(sensitivity, pitchMin, pitchMax, recenterDelay, recenterSpeed);
        }

        private void OnEnable()
        {
            lookAction.action.Enable();

            if (GameManager.Instance == null)
            {
                Debug.LogError("CameraOrbitBridge: GameManager.Instance jest null - czy scena Bootstrap była uruchomiona jako pierwsza?");
                return;
            }

            GameManager.Instance.Systems.RegisterSystem(_orbitSystem);
        }

        private void OnDisable()
        {
            lookAction.action.Disable();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.Systems.UnregisterSystem(_orbitSystem);
            }
        }

        private void Update()
        {
            if (GameplayInputLock.IsCameraLocked)
            {
                return;
            }

            _orbitSystem.AddLookInput(lookAction.action.ReadValue<Vector2>());
        }

        private void LateUpdate()
        {
            cameraAnchor.localRotation = _orbitSystem.CurrentRotation;
            cameraTransform.SetPositionAndRotation(cameraAnchor.position, cameraAnchor.rotation);
        }
    }
}
