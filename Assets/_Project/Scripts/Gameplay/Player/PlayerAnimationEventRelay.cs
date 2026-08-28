using Project.Gameplay.Combat;
using Project.Gameplay.Equipment;
using UnityEngine;

namespace Project.Gameplay.Player
{
    /// <summary>
    /// Mały przekaźnik siedzący na TYM SAMYM GameObject co Animator (model Y Bota) - Animation Event
    /// woła metody wyłącznie na obiekcie z Animatorem, a WeaponVisualBridge siedzi wyżej, na Playerze.
    /// GetComponentInParent, bo model jest dzieckiem Playera.
    /// </summary>
    public class PlayerAnimationEventRelay : MonoBehaviour
    {
        [SerializeField] private MeleeHitbox fistHitbox;

        private WeaponVisualBridge _weaponVisualBridge;

        private void Awake()
        {
            _weaponVisualBridge = GetComponentInParent<WeaponVisualBridge>();
        }

        /// <summary>Nazwa metody MUSI dokładnie zgadzać się z nazwą Animation Eventu dodanego w klipach
        /// Withdrawing Sword i Sheathing Sword w Unity.</summary>
        public void OnWeaponSocketSwap()
        {
            if (_weaponVisualBridge != null)
            {
                _weaponVisualBridge.OnWeaponSocketSwapAnimationEvent();
            }
        }

        public void OnMeleeAttackWindowStart()
        {
            GetActiveHitbox()?.Activate();
        }

        public void OnMeleeAttackWindowEnd()
        {
            GetActiveHitbox()?.Deactivate();
        }

        private MeleeHitbox GetActiveHitbox()
        {
            if (_weaponVisualBridge != null && _weaponVisualBridge.IsDrawn && _weaponVisualBridge.CurrentWeaponHitbox != null)
            {
                return _weaponVisualBridge.CurrentWeaponHitbox;
            }

            return fistHitbox;
        }
    }
}
