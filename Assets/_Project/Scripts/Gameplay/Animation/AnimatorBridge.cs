using Project.Core;
using Project.Core.Events;
using Project.Gameplay.Player;
using UnityEngine;

namespace Project.Gameplay.Animation
{
    /// <summary>
    /// MonoBehaviour "most" łączący AnimatorDriverSystem ze światem silnika. Prędkość ruchu
    /// odczytywana bezpośrednio z PlayerMotorBridge co klatkę (jak CombatBridge czyta
    /// CameraOrbitBridge.CurrentLookRotation) - nie wymaga wpisu w Script Execution Order, bo
    /// odczyt następuje w Update(), nie w OnEnable(). Attack i Death przychodzą przez EventBus,
    /// bo są to zdarzenia jednorazowe pochodzące z innych systemów (Combat, PlayerStats), nie
    /// dane odpytywane co klatkę.
    /// </summary>
    public class AnimatorBridge : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private PlayerMotorBridge playerMotorBridge;
        [SerializeField] private float speedDampTime = 0.15f;

        private AnimatorDriverSystem _animatorSystem;

        private void Awake()
        {
            _animatorSystem = new AnimatorDriverSystem(animator, speedDampTime);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<PlayerAttackPerformedEvent>(OnPlayerAttackPerformed);
            EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
            EventBus.Subscribe<WeaponDrawStateChangedEvent>(OnWeaponDrawStateChanged);

            if (GameManager.Instance == null)
            {
                Debug.LogError("AnimatorBridge: GameManager.Instance jest null - czy scena Bootstrap była uruchomiona jako pierwsza?");
                return;
            }

            GameManager.Instance.Systems.RegisterSystem(_animatorSystem);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<PlayerAttackPerformedEvent>(OnPlayerAttackPerformed);
            EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
            EventBus.Unsubscribe<WeaponDrawStateChangedEvent>(OnWeaponDrawStateChanged);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.Systems.UnregisterSystem(_animatorSystem);
            }
        }

        private void Update()
        {
            _animatorSystem.SetMoveVelocity(playerMotorBridge.CurrentVelocityX, playerMotorBridge.CurrentVelocityZ);
        }

        private void OnPlayerAttackPerformed(PlayerAttackPerformedEvent evt)
        {
            _animatorSystem.TriggerAttack();
        }

        private void OnPlayerDied(PlayerDiedEvent evt)
        {
            _animatorSystem.TriggerDeath();
        }

        private void OnWeaponDrawStateChanged(WeaponDrawStateChangedEvent evt)
        {
            _animatorSystem.SetArmed(evt.IsDrawn);

            if (evt.IsDrawn)
            {
                _animatorSystem.TriggerDraw();
            }
            else
            {
                _animatorSystem.TriggerSheathe();
            }
        }
    }
}
