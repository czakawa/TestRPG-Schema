using Project.Gameplay.Stats;
using UnityEngine;
using UnityEngine.AI;

namespace Project.Gameplay.Combat
{
    /// <summary>
    /// MonoBehaviour sterujące zachowaniem pojedynczego wroga (patrol/pościg/atak) na bazie
    /// NavMeshAgent. CELOWY wyjątek od wzorca IGameSystem: to zachowanie per-instancja przypisane
    /// do konkretnego GameObjectu wroga, nie globalny system gry pollingowany przez
    /// GameSystemsManager - dokładnie tak samo jak EnemyController jest MonoBehaviour, nie IGameSystem.
    /// Detekcja gracza (detectionRange/loseSightRange) opiera się WYŁĄCZNIE na Vector3.Distance, bez
    /// raycasta line-of-sight - to świadome uproszczenie tego etapu: wróg "wyczuwa" gracza przez
    /// ściany. Line-of-sight to osobne zlecenie do rozważenia później. loseSightRange > detectionRange
    /// celowo (histereza), żeby wróg nie migał Patrol/Chase na granicy zasięgu wykrycia.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(EnemyController))]
    [RequireComponent(typeof(EnemyAnimatorBridge))]
    public class EnemyAI : MonoBehaviour
    {
        private const float PatrolArrivalThreshold = 0.3f;
        private const float ChaseRepathInterval = 0.2f;

        [SerializeField] private Transform[] patrolPoints;
        [SerializeField] private float detectionRange = 8f;
        [SerializeField] private float loseSightRange = 12f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float attackDamage = 8f;
        [SerializeField] private float attackCooldown = 1.5f;
        [SerializeField] private float patrolWaitTime = 2f;
        [SerializeField] private MeleeHitbox attackHitbox;

        private NavMeshAgent _agent;
        private EnemyController _enemyController;
        private EnemyAnimatorBridge _animatorBridge;
        private Transform _playerTransform;
        private PlayerStatsBridge _playerStatsBridge;

        private EnemyState _currentState;
        private int _currentPatrolIndex;
        private float _stateTimer;
        private float _lastAttackTime = -999f;
        private float _lastChaseRepathTime = -999f;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _enemyController = GetComponent<EnemyController>();
            _animatorBridge = GetComponent<EnemyAnimatorBridge>();
        }

        private void Start()
        {
            // Namierzenie gracza raz, nie co klatkę - w tej scenie istnieje dokładnie jeden gracz.
            _playerStatsBridge = FindFirstObjectByType<PlayerStatsBridge>();
            _playerTransform = _playerStatsBridge != null ? _playerStatsBridge.transform : null;

            EnterPatrol();
        }

        private void Update()
        {
            if (_enemyController.IsDead)
            {
                _agent.isStopped = true;
                return;
            }

            switch (_currentState)
            {
                case EnemyState.Patrol:
                    TickPatrol();
                    break;
                case EnemyState.Chase:
                    TickChase();
                    break;
                case EnemyState.Attack:
                    TickAttack();
                    break;
            }
        }

        private void TickPatrol()
        {
            if (patrolPoints.Length > 0 && !_agent.pathPending && _agent.remainingDistance <= PatrolArrivalThreshold)
            {
                _stateTimer += Time.deltaTime;

                if (_stateTimer >= patrolWaitTime)
                {
                    _stateTimer = 0f;
                    _currentPatrolIndex = (_currentPatrolIndex + 1) % patrolPoints.Length;
                    _agent.SetDestination(patrolPoints[_currentPatrolIndex].position);
                }
            }

            if (DistanceToPlayer() < detectionRange)
            {
                EnterChase();
            }
        }

        private void TickChase()
        {
            float distanceToPlayer = DistanceToPlayer();

            if (distanceToPlayer > loseSightRange)
            {
                EnterPatrol();
                return;
            }

            if (distanceToPlayer < attackRange)
            {
                EnterAttack();
                return;
            }

            if (Time.time - _lastChaseRepathTime >= ChaseRepathInterval)
            {
                _agent.SetDestination(_playerTransform.position);
                _lastChaseRepathTime = Time.time;
            }
        }

        private void TickAttack()
        {
            if (DistanceToPlayer() > attackRange)
            {
                EnterChase();
                return;
            }

            if (Time.time - _lastAttackTime >= attackCooldown)
            {
                _lastAttackTime = Time.time;
                attackHitbox.SetDamage(attackDamage);
                _animatorBridge.TriggerAttack();
            }
        }

        private void EnterPatrol()
        {
            _currentState = EnemyState.Patrol;
            _stateTimer = 0f;
            _agent.isStopped = false;

            if (patrolPoints.Length > 0)
            {
                _agent.SetDestination(patrolPoints[_currentPatrolIndex].position);
            }
        }

        private void EnterChase()
        {
            _currentState = EnemyState.Chase;
            _agent.isStopped = false;
            _lastChaseRepathTime = -999f;
        }

        private void EnterAttack()
        {
            _currentState = EnemyState.Attack;
            _agent.isStopped = true;
        }

        /// <summary>Zwraca Mathf.Infinity, gdy gracz nie został znaleziony w Start - dzięki temu
        /// żaden próg dystansu nigdy się nie spełnia i wróg bezpiecznie zostaje w Patrol zamiast
        /// rzucić NullReferenceException.</summary>
        private float DistanceToPlayer()
        {
            return _playerTransform != null
                ? Vector3.Distance(transform.position, _playerTransform.position)
                : Mathf.Infinity;
        }

        public void OnAttackWindowStart()
        {
            attackHitbox.Activate();
        }

        public void OnAttackWindowEnd()
        {
            attackHitbox.Deactivate();
        }
    }
}
