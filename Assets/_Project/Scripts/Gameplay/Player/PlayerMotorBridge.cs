using Project.Core;
using Project.Gameplay.Stats;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Gameplay.Player
{
    /// <summary>
    /// MonoBehaviour "most" łączący czysty PlayerMotorSystem ze światem silnika: podpina CharacterController
    /// i Input Actions w Inspectorze, rejestruje system w GameSystemsManager przy włączeniu obiektu
    /// i wyrejestrowuje go przy wyłączeniu. Sam nie zawiera logiki ruchu - tylko przekazuje dane.
    /// Sprint jest tu, nie w PlayerMotorSystem, mimo że mnożnik prędkości aplikuje się w systemie:
    /// decyzja "czy sprintować w tej klatce" wymaga inputu (Shift), GameplayInputLock i wywołania
    /// TrySpendStamina na PlayerStatsSystem - same rzeczy spoza czystego systemu ruchu, dokładnie tak
    /// jak CombatBridge.GetAttackDamage() liczy obrażenia na poziomie mostu, nie w CombatSystem.
    /// PlayerMotorSystem dostaje już gotowy wynik (SetSprinting(bool)) i tylko aplikuje mnożnik.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMotorBridge : MonoBehaviour
    {
        [SerializeField] private CharacterController controller;
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string actionMapName = "Player";
        [SerializeField] private string moveActionName = "Move";
        [SerializeField] private string jumpActionName = "Jump";
        [SerializeField] private string sprintActionName = "Sprint";
        [SerializeField] private PlayerStatsBridge playerStatsBridge;

        [Header("Tuning")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float jumpHeight = 1.2f;
        [SerializeField] private float sprintMultiplier = 1.6f;
        [SerializeField] private float staminaCostPerSecond = 15f;

        private PlayerMotorSystem _motorSystem;
        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _sprintAction;

        private void Awake()
        {
            if (controller == null)
            {
                controller = GetComponent<CharacterController>();
            }

            _motorSystem = new PlayerMotorSystem(controller, transform, moveSpeed, gravity, jumpHeight, sprintMultiplier);

            InputActionMap map = inputActions.FindActionMap(actionMapName, throwIfNotFound: true);
            _moveAction = map.FindAction(moveActionName, throwIfNotFound: true);
            _jumpAction = map.FindAction(jumpActionName, throwIfNotFound: true);
            _sprintAction = map.FindAction(sprintActionName, throwIfNotFound: true);
        }

        private void OnEnable()
        {
            _moveAction.Enable();
            _jumpAction.Enable();
            _sprintAction.Enable();
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
            _sprintAction.Disable();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.Systems.UnregisterSystem(_motorSystem);
            }
        }

        private void Update()
        {
            Vector2 moveInput = GameplayInputLock.IsMovementLocked ? Vector2.zero : _moveAction.ReadValue<Vector2>();
            _motorSystem.SetMoveInput(moveInput);
            _motorSystem.SetSprinting(WantsToSprintThisFrame(moveInput));
        }

        /// <summary>
        /// Nie sprintuje stojąc w miejscu, nie sprintuje przy zablokowanym ruchu, i nie sprintuje bez
        /// wystarczającej Staminy - TrySpendStamina jest wołane tylko wtedy, gdy pierwsze dwa warunki
        /// już przeszły (żeby nie zużywać Staminy "za darmo" przy próbie sprintu, który i tak nie
        /// dojdzie do skutku z innego powodu). Gdy Stamina się skończy w trakcie trzymania Shift,
        /// TrySpendStamina zacznie zwracać false i sprint po prostu przestanie się aplikować od tej
        /// klatki - płynny powrót do normalnego biegu bez osobnego stanu "sprint wyłączony".
        /// </summary>
        private bool WantsToSprintThisFrame(Vector2 moveInput)
        {
            if (GameplayInputLock.IsMovementLocked || moveInput == Vector2.zero || !_sprintAction.IsPressed())
            {
                return false;
            }

            return playerStatsBridge != null && playerStatsBridge.Stats.TrySpendStamina(staminaCostPerSecond * Time.deltaTime);
        }

        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            _motorSystem.RequestJump();
        }
    }
}
