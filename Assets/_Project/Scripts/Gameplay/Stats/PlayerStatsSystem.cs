using Project.Core.Events;
using Project.Core.Systems;
using UnityEngine;

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
        private readonly float _staminaRegenRate;
        private readonly float _staminaRegenDelay;

        private int _strength;
        private int _dexterity;
        private int _endurance;
        private int _wisdom;
        private bool _isDead;
        private float _timeSinceStaminaUse;

        // Reszta ułamkowa nieodzwierciedlona jeszcze w Stamina.Current (int), zawsze w [0, 1).
        // Konieczna, bo koszt sprintu/regeneracja liczone są per-klatkę (np. 15 * Time.deltaTime =~
        // 0.24) - bez tego bufora naiwne zaokrąglenie do int przed każdym Vital.Add dawałoby niemal
        // zawsze 0 i sprint/regen nigdy by faktycznie nie zmieniły Stamina.Current.
        private float _staminaFraction;

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
            int startingWisdom = 10,
            float staminaRegenRate = 10f,
            float staminaRegenDelay = 2f)
        {
            _startingHealth = startingHealth;
            _startingStamina = startingStamina;
            _startingMana = startingMana;
            _startingStrength = startingStrength;
            _startingDexterity = startingDexterity;
            _startingEndurance = startingEndurance;
            _startingWisdom = startingWisdom;
            _staminaRegenRate = staminaRegenRate;
            _staminaRegenDelay = staminaRegenDelay;
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
            _isDead = false;
            _timeSinceStaminaUse = 0f;
            _staminaFraction = 0f;
        }

        public void Tick(float deltaTime)
        {
            if (Stamina.Current < Stamina.Max)
            {
                _timeSinceStaminaUse += deltaTime;

                if (_timeSinceStaminaUse >= _staminaRegenDelay)
                {
                    int previousCurrent = Stamina.Current;
                    ApplyStaminaDelta(_staminaRegenRate * deltaTime);

                    if (Stamina.Current != previousCurrent)
                    {
                        EventBus.Publish(new StatsChangedEvent());
                    }
                }
            }
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

            // Publikacja tylko przy przejściu >0 -> 0, nie przy każdym kolejnym ModifyHealth(-X)
            // wołanym, gdy HP jest już na zerze (np. wielokrotne trafienia w tej samej klatce).
            if (!_isDead && Health.Current == 0)
            {
                _isDead = true;
                EventBus.Publish(new PlayerDiedEvent());
            }
        }

        public void ModifyStamina(int delta)
        {
            Stamina.Add(delta);
            EventBus.Publish(new StatsChangedEvent());
        }

        /// <summary>
        /// Współdzielona metoda zużycia Staminy - używana zarówno przez sprint (wołana co klatkę
        /// z małą wartością: staminaCostPerSecond * deltaTime) jak i przez atak (wołana raz,
        /// z większą wartością), żeby nie duplikować logiki odejmowania w dwóch miejscach. Sprawdza
        /// tylko Stamina.Current (int), nie uwzględniając _staminaFraction - to sprawdzenie jest więc
        /// lekko konserwatywne (może odmówić przy &lt;1 jednostce zapasu), ale nigdy nie pozwoli
        /// zejść poniżej zera. Resetuje _timeSinceStaminaUse, więc regeneracja zacznie się dopiero
        /// po staminaRegenDelay od OSTATNIEGO udanego zużycia (sprint trzymany ciągle bez przerwy
        /// odsuwa start regeneracji w nieskończoność, zgodnie z zamierzeniem).
        /// </summary>
        public bool TrySpendStamina(float amount)
        {
            if (Stamina.Current < amount)
            {
                return false;
            }

            ApplyStaminaDelta(-amount);
            _timeSinceStaminaUse = 0f;
            EventBus.Publish(new StatsChangedEvent());
            return true;
        }

        /// <summary>
        /// Wspólny mechanizm dla TrySpendStamina i regeneracji w Tick: dokłada deltę (dodatnią lub
        /// ujemną) do _staminaFraction, wyciąga z niej całkowitą liczbę jednostek przez Mathf.FloorToInt
        /// (poprawne dla obu znaków - zachowuje niezmiennik Stamina.Current == floor(prawdziwa wartość))
        /// i tylko tę całkowitą część aplikuje przez istniejące Vital.Add (zachowując jego clamp do
        /// [0, Max]). Reszta ułamkowa zawsze zostaje w [0, 1).
        /// </summary>
        private void ApplyStaminaDelta(float delta)
        {
            float trueDelta = _staminaFraction + delta;
            int wholeUnits = Mathf.FloorToInt(trueDelta);
            _staminaFraction = trueDelta - wholeUnits;

            if (wholeUnits != 0)
            {
                Stamina.Add(wholeUnits);
            }
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
