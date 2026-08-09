using UnityEngine;
using UnityEngine.AI;

namespace Project.Gameplay.Combat
{
    /// <summary>
    /// MonoBehaviour sterujący Animatorem pojedynczego wroga - analogicznie do EnemyAI, per-instancja,
    /// NIE IGameSystem. Prędkość ruchu czytana bezpośrednio z NavMeshAgent.velocity co klatkę (jak
    /// AnimatorBridge czyta PlayerMotorBridge.CurrentVelocityZ). Attack i Death wywoływane
    /// bezpośrednio przez EnemyAI/EnemyController (referencja na tym samym GameObject), nie przez
    /// EventBus - to wewnętrzna komunikacja jednego wroga ze swoim własnym Animatorem, nie coś,
    /// co inne systemy muszą obserwować (to zarezerwowane dla EnemyDiedEvent).
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyAnimatorBridge : MonoBehaviour
    {
        private static readonly int SpeedParam = Animator.StringToHash("Speed");
        private static readonly int AttackTriggerParam = Animator.StringToHash("Attack");
        private static readonly int DeathTriggerParam = Animator.StringToHash("Death");

        [SerializeField] private float speedDampTime = 0.15f;

        private Animator _animator;
        private NavMeshAgent _agent;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _agent = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            _animator.SetFloat(SpeedParam, _agent.velocity.magnitude, speedDampTime, Time.deltaTime);
        }

        public void TriggerAttack()
        {
            _animator.SetTrigger(AttackTriggerParam);
        }

        public void TriggerDeath()
        {
            _animator.SetTrigger(DeathTriggerParam);
        }
    }
}
