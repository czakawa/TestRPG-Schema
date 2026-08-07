using Project.Core.Events;
using Project.Core.Systems;

namespace Project.Gameplay.Stats
{
    /// <summary>
    /// Cztery atrybuty postaci, do wykorzystania przez <see cref="PlayerStatsSystem.SetAttribute"/>
    /// i przyszły system rozwoju postaci (punkty do rozdania).
    /// </summary>
    public enum AttributeType
    {
        Strength,
        Dexterity,
        Endurance,
        Wisdom
    }

    /// <summary>
    /// Prosta wartość "current/max" (zdrowie, stamina, mana), clampowana do [0, Max] przy każdej
    /// zmianie. Czysta struktura danych bez własnej logiki zdarzeń - właściciel (PlayerStatsSystem)
    /// decyduje kiedy publikować zmianę.
    /// </summary>
    public class Vital
    {
        public int Current;
        public int Max;

        public Vital(int max, int current)
        {
            Max = max;
            Current = Clamp(current);
        }

        public void SetMax(int max)
        {
            Max = max;
            Current = Clamp(Current);
        }

        public void Add(int amount)
        {
            Current = Clamp(Current + amount);
        }

        private int Clamp(int value)
        {
            if (value < 0)
            {
                return 0;
            }

            return value > Max ? Max : value;
        }
    }

    /// <summary>
    /// Czysta klasa C# (nie MonoBehaviour) zarządzająca statystykami gracza: trzema Vitalami
    /// (Health/Stamina/Mana) i czterema atrybutami (Strength/Dexterity/Endurance/Wisdom). Brak
    /// regeneracji w czasie i logiki punktów do rozdania - to zakres przyszłego systemu rozwoju
    /// postaci / Combat System. Tutaj tylko przechowywanie stanu i publikacja zmian przez EventBus,
    /// analogicznie do CurrencySystem/InventorySystem.
    /// </summary>
    public class PlayerStatsSystem : IGameSystem
    {
        private readonly int _startingHealth;
        private readonly int _startingStamina;
        private readonly int _startingMana;
        private readonly int _startingStrength;
        private readonly int _startingDexterity;
        private readonly int _startingEndurance;
        private readonly int _startingWisdom;

        private int _strength;
        private int _dexterity;
        private int _endurance;
        private int _wisdom;

        public Vital Health { get; private set; }
        public Vital Stamina { get; private set; }
        public Vital Mana { get; private set; }

        public int Strength => _strength;
        public int Dexterity => _dexterity;
        public int Endurance => _endurance;
        public int Wisdom => _wisdom;

        public PlayerStatsSystem(
            int startingHealth = 100,
            int startingStamina = 100,
            int startingMana = 100,
            int startingStrength = 10,
            int startingDexterity = 10,
            int startingEndurance = 10,
            int startingWisdom = 10)
        {
            _startingHealth = startingHealth;
            _startingStamina = startingStamina;
            _startingMana = startingMana;
            _startingStrength = startingStrength;
            _startingDexterity = startingDexterity;
            _startingEndurance = startingEndurance;
            _startingWisdom = startingWisdom;
        }

        public void Initialize()
        {
            Health = new Vital(_startingHealth, _startingHealth);
            Stamina = new Vital(_startingStamina, _startingStamina);
            Mana = new Vital(_startingMana, _startingMana);

            _strength = _startingStrength;
            _dexterity = _startingDexterity;
            _endurance = _startingEndurance;
            _wisdom = _startingWisdom;
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

        public void ModifyHealth(int delta)
        {
            Health.Add(delta);
            EventBus.Publish(new StatsChangedEvent());
        }

        public void ModifyStamina(int delta)
        {
            Stamina.Add(delta);
            EventBus.Publish(new StatsChangedEvent());
        }

        public void ModifyMana(int delta)
        {
            Mana.Add(delta);
            EventBus.Publish(new StatsChangedEvent());
        }

        /// <summary>
        /// Ustawia wartość atrybutu bezpośrednio - bez logiki punktów do rozdania. Do wykorzystania
        /// przez przyszły system rozwoju postaci.
        /// </summary>
        public void SetAttribute(AttributeType type, int value)
        {
            switch (type)
            {
                case AttributeType.Strength:
                    _strength = value;
                    break;
                case AttributeType.Dexterity:
                    _dexterity = value;
                    break;
                case AttributeType.Endurance:
                    _endurance = value;
                    break;
                case AttributeType.Wisdom:
                    _wisdom = value;
                    break;
            }

            EventBus.Publish(new StatsChangedEvent());
        }
    }
}
