using System.Collections.Generic;
using Project.Core.Events;
using UnityEngine;

namespace Project.Gameplay.Combat
{
    /// <summary>
    /// Uniwersalny collider-trigger zadający obrażenia dowolnemu IDamageable, który w niego wejdzie
    /// w oknie czasowym Activate()/Deactivate() - wołanym przez Animation Event w konkretnych klatkach
    /// animacji ataku (nie na starcie/końcu całej animacji, tylko w momencie faktycznego zamachu).
    /// Ten sam komponent jest używany na modelu broni gracza, na pięści gracza (zawsze obecny pod
    /// kością RightHand, aktywny tylko podczas Attack_Unarmed) i na wrogu - dzięki IDamageable
    /// zaimplementowanemu teraz zarówno przez EnemyController jak i PlayerStatsBridge. Śledzi już
    /// trafione cele w bieżącej aktywacji (_hitThisActivation), żeby jeden zamach nie zadał obrażeń
    /// wielokrotnie temu samemu celowi przy nakładających się klatkach collidera.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class MeleeHitbox : MonoBehaviour
    {
        [SerializeField] private LayerMask targetLayer;

        private Collider _collider;
        private float _pendingDamage;
        private bool _isActive;
        private readonly HashSet<IDamageable> _hitThisActivation = new HashSet<IDamageable>();

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _collider.isTrigger = true;
            _collider.enabled = false;
        }

        /// <summary>Wołane przez właściciela (CombatBridge/EnemyAI) w momencie rozpoczęcia ataku,
        /// ZANIM animacja faktycznie zacznie się odtwarzać - obrażenia są "zamrożone" na czas
        /// całego zamachu, nawet jeśli broń zmieni się w międzyczasie (nie powinno się zdarzyć,
        /// ale dla pewności).</summary>
        public void SetDamage(float damage)
        {
            _pendingDamage = damage;
        }

        public void Activate()
        {
            _hitThisActivation.Clear();
            _isActive = true;
            _collider.enabled = true;
        }

        public void Deactivate()
        {
            _isActive = false;
            _collider.enabled = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_isActive)
            {
                return;
            }

            if ((targetLayer.value & (1 << other.gameObject.layer)) == 0)
            {
                return;
            }

            IDamageable target = other.GetComponentInParent<IDamageable>();
            if (target == null || target.IsDead || _hitThisActivation.Contains(target))
            {
                return;
            }

            _hitThisActivation.Add(target);
            target.TakeDamage(_pendingDamage);

            // Reużyty event z poprzedniego systemu (SphereCast) - nazwa historyczna, ale sygnatura
            // (GameObject, float) pasuje uniwersalnie. Subskrybenci (np. EnemyHealthBarUI) i tak
            // filtrują po dopasowaniu własnego GameObjectu, więc publikacja przy trafieniu gracza
            // przez wroga jest neutralna (nikt nie dopasuje siebie i nic się nie stanie).
            EventBus.Publish(new EnemyDamagedEvent(other.gameObject, _pendingDamage));
        }
    }
}
