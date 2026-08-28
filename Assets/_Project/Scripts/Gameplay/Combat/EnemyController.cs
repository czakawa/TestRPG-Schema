using Project.Core.Events;
using UnityEngine;

namespace Project.Gameplay.Combat
{
    /// <summary>
    /// MonoBehaviour prostego, statycznego wroga: przechowuje HP i implementuje IDamageable. Brak
    /// jakiegokolwiek AI/ruchu - to zakres kolejnego zlecenia. Publikuje EnemyDiedEvent przez EventBus
    /// zamiast bezpośrednio wołać UI/inne systemy, żeby przyszły loot/quest mógł zareagować bez
    /// referencji do tego konkretnego obiektu. Zniszczenie obiektu jest opóźnione o 0.1s po publikacji
    /// eventu, żeby subskrybenci zdążyli odczytać jego stan w tej samej klatce.
    /// </summary>
    public class EnemyController : MonoBehaviour, IDamageable
    {
        [SerializeField] private float maxHealth = 50f;
        [SerializeField] private float deathDestroyDelay = 2.5f;
        [SerializeField] private int xpReward = 10;

        private float _currentHealth;
        private EnemyAnimatorBridge _animatorBridge;

        public bool IsDead { get; private set; }

        /// <summary>Do odczytu przez EnemyHealthBarUI - aktualne HP wroga.</summary>
        public float CurrentHealth => _currentHealth;

        /// <summary>Do odczytu przez EnemyHealthBarUI - maksymalne HP wroga.</summary>
        public float MaxHealth => maxHealth;

        public int XpReward => xpReward;

        private void Awake()
        {
            _currentHealth = maxHealth;
            _animatorBridge = GetComponent<EnemyAnimatorBridge>();
        }

        public void TakeDamage(float amount)
        {
            if (IsDead)
            {
                return;
            }

            _currentHealth = Mathf.Max(0f, _currentHealth - amount);

            if (_currentHealth <= 0f)
            {
                IsDead = true;
                _animatorBridge.TriggerDeath();
                EventBus.Publish(new EnemyDiedEvent(gameObject));
                Destroy(gameObject, deathDestroyDelay);
            }
        }
    }
}
