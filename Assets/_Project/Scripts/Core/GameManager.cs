using Project.Core.States;
using Project.Core.Systems;
using UnityEngine;

namespace Project.Core
{
    /// <summary>
    /// Jedyny prawdziwy Singleton w projekcie (DontDestroyOnLoad). Inicjalizuje GameStateMachine
    /// i GameSystemsManager oraz wystawia publiczne API do zmiany stanu gry. Żaden inny system
    /// nie powinien mieć własnego static Instance - komunikacja między nimi idzie przez EventBus
    /// lub przez referencje wstrzyknięte z tego miejsca.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private GameSystemsManager systemsManager;

        private GameStateMachine _stateMachine;

        /// <summary>Dostęp do centralnej pętli systemów, np. do rejestrowania nowych IGameSystem.</summary>
        public GameSystemsManager Systems => systemsManager;

        /// <summary>Aktualny stan gry.</summary>
        public GameStateType CurrentState => _stateMachine.CurrentStateType;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (systemsManager == null)
            {
                systemsManager = GetComponentInChildren<GameSystemsManager>();
            }

            InitializeStateMachine();
        }

        private void InitializeStateMachine()
        {
            _stateMachine = new GameStateMachine();
            _stateMachine.RegisterState(GameStateType.MainMenu, new MainMenuState(systemsManager));
            _stateMachine.RegisterState(GameStateType.Gameplay, new GameplayState(systemsManager));
            _stateMachine.RegisterState(GameStateType.Pause, new PauseState(systemsManager));
            _stateMachine.RegisterState(GameStateType.GameOver, new GameOverState(systemsManager));
        }

        private void Update()
        {
            _stateMachine?.Tick(Time.deltaTime);
        }

        /// <summary>Publiczne API do przełączania stanu gry z dowolnego miejsca (UI, systemy, Bootstrapper).</summary>
        public void ChangeState(GameStateType newState)
        {
            _stateMachine.ChangeState(newState);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
