using Project.Core.Events;
using Project.Core.Systems;
using UnityEngine;
namespace Project.Gameplay.Economy
{
    /// <summary>
    /// Czysta klasa C# (nie MonoBehaviour) zarządzająca ilością złota gracza. Niezależna od
    /// InventorySystem - waluta nie jest przedmiotem ekwipunku, tylko osobnym, równoległym stanem.
    /// </summary>
    public class CurrencySystem : IGameSystem
    {
        private readonly int _startingGold;
        private int _gold;

        public int Gold => _gold;

        public CurrencySystem(int startingGold = 0)
        {
            _startingGold = startingGold;
        }

        public void Initialize()
        {
            _gold = _startingGold;
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

        /// <summary>Dodaje złoto (amount musi być dodatnie) i publikuje CurrencyChangedEvent.</summary>
        public void AddGold(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            _gold += amount;
            EventBus.Publish(new CurrencyChangedEvent(_gold));
            //Debug.Log($"AddGold wywołane, nowa wartość: {_gold}, instancja: {this.GetHashCode()}");
        }

        /// <summary>Próbuje wydać złoto. Zwraca false (bez zmiany stanu) jeśli amount &lt;= 0 lub za mało złota.</summary>
        public bool TrySpendGold(int amount)
        {
            if (amount <= 0 || _gold < amount)
            {
                return false;
            }

            _gold -= amount;
            EventBus.Publish(new CurrencyChangedEvent(_gold));
            return true;
        }
    }
}
