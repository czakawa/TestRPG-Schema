using Project.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Gameplay.Player
{
    /// <summary>
    /// MonoBehaviour "most" łączący czysty PlayerMotorSystem ze światem silnika: podpina CharacterController
    /// i Input Actions w Inspectorze, rejestruje system w GameSystemsManager przy włączeniu obiektu
    /// i wyrejestrowuje go przy wyłączeniu. Sam nie zawiera logiki ruchu - tylko przekazuje dane.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMotorBridge : MonoBehaviour
    {
        [SerializeField] private CharacterController controller;
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string actionMapName = "Player";
        [SerializeField] private string moveActionName = "Move";
        [SerializeField] private string jumpActionName = "Jump";

        [Header("Tuning")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float jumpHeight = 1.2f;

        private PlayerMotorSystem _motorSystem;
        private InputAction _moveAction;
        private InputAction _jumpAction;

        private void Awake()
        {
            if (controller == null)
            {
                controller = GetComponent<CharacterController>();
            }

            _motorSystem = new PlayerMotorSystem(controller, transform, moveSpeed, gravity, jumpHeight);

            InputActionMap map = inputActions.FindActionMap(actionMapName, throwIfNotFound: true);
            _moveAction = map.FindAction(moveActionName, throwIfNotFound: true);
            _jumpAction = map.FindAction(jumpActionName, throwIfNotFound: true);
        }

        private void OnEnable()
        {
            _moveAction.Enable();
            _jumpAction.Enable();
            _jumpAction.performed += OnJumpPerformed;

            if (GameManager.Instance == null)
            {
                Debug.LogError("PlayerMotorBridge: GameManager.Instance jest null - czy scena Bootstrap była uruchomiona jako pierwsza?");
                return;
            }

            GameManager.Instance.Systems.RegisterSystem(_motorSystem);
        }

        private void OnDisable()
        {
            _jumpAction.performed -= OnJumpPerformed;
            _moveAction.Disable();
            _jumpAction.Disable();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.Systems.UnregisterSystem(_motorSystem);
            }
        }

        private void Update()
        {
            _motorSystem.SetMoveInput(_moveAction.ReadValue<Vector2>());
        }

        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            _motorSystem.RequestJump();
        }
    }
}
