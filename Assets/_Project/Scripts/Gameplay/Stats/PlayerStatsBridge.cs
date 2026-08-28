using Project.Core;
using Project.Core.Events;
using Project.Data;
using Project.Gameplay.Combat;
using Project.Gameplay.Equipment;
using UnityEngine;

namespace Project.Gameplay.Stats
{
    /// <summary>
    /// MonoBehaviour "most" łączący czysty PlayerStatsSystem ze światem silnika: rejestruje system
    /// w GameSystemsManager przy włączeniu obiektu i wyrejestrowuje przy wyłączeniu. Wystawia
    /// referencję do PlayerStatsSystem, żeby UI (CharacterPanelUI) i przyszłe systemy
    /// (Combat, rozwój postaci) mogły z niego korzystać. Redukcja obrażeń przez pancerz
    /// (<see cref="TakeDamage"/>) żyje tu, na poziomie mostu, a nie w czystym PlayerStatsSystem -
    /// analogicznie do CombatBridge.GetAttackDamage(): odczytanie EquippedArmor wymagałoby
    /// przekazania referencji do EquipmentSystem w konstruktorze PlayerStatsSystem, co sprzęgałoby
    /// ze sobą dwa czyste systemy bez potrzeby (PlayerStatsSystem nie musi nic wiedzieć
    /// o ekwipunku - tylko most, który i tak już zna oba, potrzebuje tej wiedzy).
    /// </summary>
    public class PlayerStatsBridge : MonoBehaviour, IDamageable
    {
        [SerializeField] private int startingHealth = 100;
        [SerializeField] private int startingStamina = 100;
        [SerializeField] private int startingMana = 100;
        [SerializeField] private int startingStrength = 10;
        [SerializeField] private int startingDexterity = 10;
        [SerializeField] private int startingEndurance = 10;
        [SerializeField] private int startingWisdom = 10;
        [SerializeField] private float staminaRegenRate = 10f;
        [SerializeField] private float staminaRegenDelay = 2f;
        [SerializeField] private int xpPerLevelBase = 100;
        [SerializeField] private int xpPerLevelIncrease = 50;
        [SerializeField] private int learningPointsPerLevel = 5;
        [SerializeField] private EquipmentBridge equipmentBridge;

        private PlayerStatsSystem _statsSystem;

        /// <summary>Referencja do systemu statystyk gracza - do użytku przez UI (CharacterPanelUI) i przyszłe systemy.</summary>
        public PlayerStatsSystem Stats => _statsSystem;

        /// <summary>Implementacja IDamageable - deleguje do PlayerStatsSystem.IsDead. Potrzebne, żeby
        /// MeleeHitbox (uniwersalny dla gracza i wroga) mógł traktować gracza tak samo jak EnemyController.</summary>
        public bool IsDead => _statsSystem.IsDead;

        private void Awake()
        {
            _statsSystem = new PlayerStatsSystem(
                startingHealth,
                startingStamina,
                startingMana,
                startingStrength,
                startingDexterity,
                startingEndurance,
                startingWisdom,
                staminaRegenRate,
                staminaRegenDelay,
                xpPerLevelBase,
                xpPerLevelIncrease,
                learningPointsPerLevel);
        }

        private void OnEnable()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("PlayerStatsBridge: GameManager.Instance jest null - czy scena Bootstrap była uruchomiona jako pierwsza?");
                return;
            }

            GameManager.Instance.Systems.RegisterSystem(_statsSystem);
            EventBus.Subscribe<EnemyDiedEvent>(OnEnemyDied);
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.Systems.UnregisterSystem(_statsSystem);
            }

            EventBus.Unsubscribe<EnemyDiedEvent>(OnEnemyDied);
        }

        /// <summary>
        /// Zadaje obrażenia z redukcją przez wyekwipowany pancerz (EquippedArmor.ArmorValue) -
        /// DODATKOWA metoda obok ModifyHealth, nie jej zamiennik: ModifyHealth zostaje publiczne
        /// do przypadków, które celowo NIE powinny przechodzić przez redukcję pancerza (np. leczenie
        /// miksturami - wartość dodatnia i tak nigdy by przez redukcję nie przeszła, ale też direct
        /// damage spoza walki, jeśli kiedyś powstanie). Minimalny próg 1 obrażenia, żeby pancerz
        /// nigdy nie dawał pełnej nieśmiertelności. equipmentBridge.Equipment jest odczytywane
        /// dopiero tutaj, nie w Awake/OnEnable, więc EquipmentBridge nie wymaga wpisu w Script
        /// Execution Order względem PlayerStatsBridge (ten sam wzorzec co CombatBridge.GetAttackDamage()).
        /// </summary>
        public void TakeDamage(float rawDamage)
        {
            ItemData equippedArmor = equipmentBridge != null && equipmentBridge.Equipment != null
                ? equipmentBridge.Equipment.EquippedArmor
                : null;

            float finalDamage = equippedArmor != null
                ? Mathf.Max(1f, rawDamage - equippedArmor.ArmorValue)
                : rawDamage;

            _statsSystem.ModifyHealth(-Mathf.RoundToInt(finalDamage));
        }

        private void OnEnemyDied(EnemyDiedEvent evt)
        {
            EnemyController enemyController = evt.Enemy.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                _statsSystem.AddExperience(enemyController.XpReward);
            }
        }
    }
}
