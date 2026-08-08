using Project.Core.Events;
using Project.Core.Systems;
using UnityEngine;

namespace Project.Gameplay.Combat
{
    /// <summary>
    /// Czysta klasa C# (nie MonoBehaviour) wykonująca atak gracza w zasięgu. W przeciwieństwie do
    /// InteractionSystem nie pollinguje w Tick - atak jest wyzwalany bezpośrednio przez CombatBridge
    /// przy wciśnięciu akcji Attack, więc Tick/FixedTick pozostają puste (patrz <see cref="PerformAttack"/>).
    /// Używa Physics.SphereCastNonAlloc zamiast Raycast, żeby trafienie w melee nie wymagało
    /// pikselowo dokładnego wycelowania w środek modelu wroga, z prealokowanym buforem RaycastHit[]
    /// dokładnie jak InteractionSystem - zero alokacji na atak.
    /// </summary>
    public class CombatSystem : IGameSystem
    {
        private const float AttackRadius = 0.5f;

        private readonly float _attackRange;
        private readonly LayerMask _enemyLayer;
        private readonly RaycastHit[] _hitBuffer = new RaycastHit[1];

        public CombatSystem(float attackRange, LayerMask enemyLayer)
        {
            _attackRange = attackRange;
            _enemyLayer = enemyLayer;
        }

        public void Initialize()
        {
        }

        public void Tick(float deltaTime)
        {
        }

        public void FixedTick(float fixedDeltaTime)
        {
        }

        public void Shutdown()
        {
        }

        /// <summary>
        /// Wołane przez CombatBridge po naciśnięciu akcji Attack - wykonuje atak z podanego punktu
        /// i kierunku. Obrażenia są parametrem metody, nie konstruktora - CombatBridge wylicza je na
        /// nowo przy każdym ataku (broń z EquippedWeapon albo obrażenia gołych pięści), więc system
        /// nie może mieć ich zamrożonych w chwili konstrukcji. Trafienie w obiekt z IDamageable zadaje
        /// obrażenia i publikuje EnemyDamagedEvent; brak trafienia lub trafienie w obiekt bez
        /// IDamageable/już martwy nic nie robi.
        /// </summary>
        public void PerformAttack(Vector3 origin, Vector3 direction, float damage)
        {
            int hitCount = Physics.SphereCastNonAlloc(
                origin,
                AttackRadius,
                direction,
                _hitBuffer,
                _attackRange,
                _enemyLayer,
                QueryTriggerInteraction.Ignore);

            if (hitCount == 0)
            {
                return;
            }

            GameObject hitGameObject = _hitBuffer[0].collider.gameObject;
            IDamageable target = hitGameObject.GetComponentInParent<IDamageable>();

            if (target == null || target.IsDead)
            {
                return;
            }

            target.TakeDamage(damage);
            EventBus.Publish(new EnemyDamagedEvent(hitGameObject, damage));
        }
    }
}
