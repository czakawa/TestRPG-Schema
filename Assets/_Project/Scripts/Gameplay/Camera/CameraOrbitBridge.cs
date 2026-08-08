using Project.Core;
using Project.Core.States;
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
        [SerializeField] private Transform playerTransform;
        [SerializeField] private InputActionReference lookAction;

        [Header("Tuning")]
        [SerializeField] private float sensitivity = 0.1f;
        [SerializeField] private float pitchMin = -20f;
        [SerializeField] private float pitchMax = 60f;

        private CameraOrbitSystem _orbitSystem;

        /// <summary>Bieżąca rotacja orbitalna, świeża co klatkę (bez opóźnienia LateUpdate) - do użytku
        /// przez systemy potrzebujące kierunku patrzenia kamery przed jej fizycznym zaktualizowaniem, np. InteractionBridge.</summary>
        public Quaternion CurrentLookRotation => _orbitSystem.CurrentRotation;

        private void Awake()
        {
            _orbitSystem = new CameraOrbitSystem(sensitivity, pitchMin, pitchMax);
            _orbitSystem.SetInitialYaw(playerTransform.eulerAngles.y);
        }

        private void OnEnable()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

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
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            lookAction.action.Disable();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.Systems.UnregisterSystem(_orbitSystem);
            }
        }

        /// <summary>
        /// Guard na CurrentState == GameOver jest tu konieczny osobno od GameplayInputLock:
        /// AddLookInput mutuje stan CameraOrbitSystem bezpośrednio z tego Update(), nie przez
        /// GameSystemsManager.Tick, więc SetSystemsActive(false) w GameOverState w ogóle go nie
        /// dotyczy - bez tego sprawdzenia kamera dalej obracałaby się swobodnie na ekranie Game Over.
        /// </summary>
        private void Update()
        {
            bool shouldLockCursor = !GameplayInputLock.IsCameraLocked;
            if (shouldLockCursor && Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else if (!shouldLockCursor && Cursor.lockState != CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (GameplayInputLock.IsCameraLocked)
            {
                return;
            }

            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameStateType.GameOver)
            {
                return;
            }

            _orbitSystem.AddLookInput(lookAction.action.ReadValue<Vector2>());
            playerTransform.rotation = Quaternion.Euler(0f, _orbitSystem.Yaw, 0f);
        }

        private void LateUpdate()
        {
            cameraAnchor.localRotation = Quaternion.Euler(_orbitSystem.Pitch, 0f, 0f);
            cameraTransform.SetPositionAndRotation(cameraAnchor.position, cameraAnchor.rotation);
        }
    }
}
