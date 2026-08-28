using Project.Core;
using Project.Core.Events;
using Project.Core.States;
using Project.Gameplay.Equipment;
using Project.Gameplay.Stats;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Gameplay.Combat
{
    /// <summary>
    /// MonoBehaviour "most" obsługujący atak gracza. Nie zawiera już żadnego SphereCasta ani
    /// koncepcji zasięgu liczonego z punktu startowego - trafienie wykrywa MeleeHitbox (broń albo
    /// pięść) aktywowany w konkretnej klatce animacji przez PlayerAnimationEventRelay. Ten bridge
    /// tylko: sprawdza cooldown/blokady/Staminę, ustawia obrażenia na WŁAŚCIWYM hitboxie (broń jeśli
    /// dobyta, inaczej pięść), i publikuje trigger animacji.
    /// </summary>
    public class CombatBridge : MonoBehaviour
    {
        private const string AttackLockReason = "Attack";

        [SerializeField] private EquipmentBridge equipmentBridge;
        [SerializeField] private WeaponVisualBridge weaponVisualBridge;
        [SerializeField] private PlayerStatsBridge playerStatsBridge;
        [SerializeField] private MeleeHitbox fistHitbox;
        [SerializeField] private float baseUnarmedDamage = 5f;
        [SerializeField] private float attackStaminaCost = 15f;
        [SerializeField] private float attackCooldown = 0.8f;
        [SerializeField] private InputActionReference attackAction;

        private float _lastAttackTime = -999f;

        private void OnEnable()
        {
            attackAction.action.Enable();
            attackAction.action.performed += OnAttackPerformed;
        }

        private void OnDisable()
        {
            attackAction.action.performed -= OnAttackPerformed;
            attackAction.action.Disable();

            CancelInvoke(nameof(UnlockMovementAfterAttack));
            GameplayInputLock.UnlockMovement(AttackLockReason);
        }

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

            _lastAttackTime = Time.time;

            MeleeHitbox activeHitbox = GetActiveHitbox();
            if (activeHitbox != null)
            {
                activeHitbox.SetDamage(GetAttackDamage());
            }

            EventBus.Publish(new PlayerAttackPerformedEvent());

            GameplayInputLock.LockMovement(AttackLockReason);
            CancelInvoke(nameof(UnlockMovementAfterAttack));
            Invoke(nameof(UnlockMovementAfterAttack), attackCooldown);
        }

        private void UnlockMovementAfterAttack()
        {
            GameplayInputLock.UnlockMovement(AttackLockReason);
        }

        private MeleeHitbox GetActiveHitbox()
        {
            if (IsWeaponActive() && weaponVisualBridge.CurrentWeaponHitbox != null)
            {
                return weaponVisualBridge.CurrentWeaponHitbox;
            }

            return fistHitbox;
        }

        private float GetAttackDamage()
        {
            if (IsWeaponActive() && equipmentBridge.Equipment.EquippedWeapon != null)
            {
                return equipmentBridge.Equipment.EquippedWeapon.DamageValue;
            }

            return baseUnarmedDamage;
        }

        private bool IsWeaponActive()
        {
            return equipmentBridge != null
                && equipmentBridge.Equipment != null
                && weaponVisualBridge != null
                && weaponVisualBridge.IsDrawn;
        }
    }
}
