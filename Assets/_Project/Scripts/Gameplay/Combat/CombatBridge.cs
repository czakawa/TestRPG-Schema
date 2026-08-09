using Project.Core;
using Project.Core.Events;
using Project.Core.States;
using Project.Gameplay.Camera;
using Project.Gameplay.Equipment;
using Project.Gameplay.Stats;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Gameplay.Combat
{
    /// <summary>
    /// MonoBehaviour "most" łączący czysty CombatSystem ze światem silnika, analogicznie do
    /// InteractionBridge: podpina punkt startowy ataku, zasięg, warstwę wrogów i akcję Attack
    /// w Inspectorze, rejestruje system w GameSystemsManager przy włączeniu obiektu i wyrejestrowuje
    /// przy wyłączeniu. W przeciwieństwie do InteractionBridge nie przekazuje do systemu delegatów
    /// odczytujących pozycję/kierunek co klatkę (CombatSystem nie pollinguje w Tick) - origin/kierunek
    /// są odczytane raz, w momencie wciśnięcia Attack, i przekazane bezpośrednio do PerformAttack.
    /// Obrażenia też są wyliczane dopiero w tym momencie (EquippedWeapon.DamageValue albo
    /// baseUnarmedDamage), zamiast raz przy konstrukcji CombatSystem, żeby zmiana broni w trakcie
    /// gry działała natychmiast bez przebudowy systemu.
    /// </summary>
    public class CombatBridge : MonoBehaviour
    {
        private const string AttackLockReason = "Attack";

        [SerializeField] private Transform attackOrigin;
        [SerializeField] private CameraOrbitBridge cameraOrbitBridge;
        [SerializeField] private EquipmentBridge equipmentBridge;
        [SerializeField] private PlayerStatsBridge playerStatsBridge;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float baseUnarmedDamage = 5f;
        [SerializeField] private float attackStaminaCost = 15f;
        [SerializeField] private float attackCooldown = 0.8f;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private InputActionReference attackAction;

        private CombatSystem _combatSystem;
        private float _lastAttackTime = -999f;

        private void Awake()
        {
            _combatSystem = new CombatSystem(attackRange, enemyLayer);
        }

        private void OnEnable()
        {
            attackAction.action.Enable();
            attackAction.action.performed += OnAttackPerformed;

            if (GameManager.Instance == null)
            {
                Debug.LogError("CombatBridge: GameManager.Instance jest null - czy scena Bootstrap była uruchomiona jako pierwsza?");
                return;
            }

            GameManager.Instance.Systems.RegisterSystem(_combatSystem);
        }

        private void OnDisable()
        {
            attackAction.action.performed -= OnAttackPerformed;
            attackAction.action.Disable();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.Systems.UnregisterSystem(_combatSystem);
            }

            CancelInvoke(nameof(UnlockMovementAfterAttack));
            GameplayInputLock.UnlockMovement(AttackLockReason);
        }

        /// <summary>
        /// Blokada ruchu (GameplayInputLock.IsMovementLocked) jest tu celowo reużyta zamiast nowego
        /// klucza blokady - z otwartym ekwipunkiem/panelem postaci/questlogiem lub w trakcie dialogu
        /// gracz i tak nie powinien móc atakować, dokładnie z tych samych powodów co nie powinien
        /// się poruszać. Dodatkowy guard na CurrentState == GameOver jest konieczny osobno: atak jest
        /// wyzwalany z callbacku Input Actions (attackAction.performed), nie z pętli GameSystemsManager,
        /// więc SetSystemsActive(false) w GameOverState go nie zatrzymuje - bez tego sprawdzenia gracz
        /// mógłby dalej zadawać obrażenia wrogom z poziomu ekranu Game Over. Sprawdzenie Staminy
        /// (TrySpendStamina) jest CELOWO ostatnim guardem, po pozostałych: to jedyny z tej listy,
        /// który ma efekt uboczny przy sukcesie (zużywa Staminę), więc nie ma sensu go wołać, jeśli
        /// atak i tak nie dojdzie do skutku z innego powodu.
        /// </summary>
        private void OnAttackPerformed(InputAction.CallbackContext context)
        {
            if (Time.time - _lastAttackTime < attackCooldown)
            {
                return;
            }

            if (GameplayInputLock.IsMovementLocked)
            {
                return;
            }

            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameStateType.GameOver)
            {
                return;
            }

            if (playerStatsBridge == null || !playerStatsBridge.Stats.TrySpendStamina(attackStaminaCost))
            {
                return;
            }

            _combatSystem.PerformAttack(attackOrigin.position, cameraOrbitBridge.CurrentLookRotation * Vector3.forward, GetAttackDamage());
            EventBus.Publish(new PlayerAttackPerformedEvent());
            _lastAttackTime = Time.time;
            GameplayInputLock.LockMovement(AttackLockReason);
            Invoke(nameof(UnlockMovementAfterAttack), attackCooldown);
        }

        /// <summary>Odczytane dopiero tutaj, nie w Awake/OnEnable - tak samo jak cameraOrbitBridge
        /// w OnAttackPerformed - dlatego EquipmentBridge nie wymaga wpisu w Script Execution Order
        /// względem CombatBridge, mimo że jest kolejnym mostem, od którego ten most zależy.</summary>
        private float GetAttackDamage()
        {
            if (equipmentBridge != null && equipmentBridge.Equipment != null && equipmentBridge.Equipment.EquippedWeapon != null)
            {
                return equipmentBridge.Equipment.EquippedWeapon.DamageValue;
            }

            return baseUnarmedDamage;
        }

        private void UnlockMovementAfterAttack()
        {
            GameplayInputLock.UnlockMovement(AttackLockReason);
        }
    }
}
