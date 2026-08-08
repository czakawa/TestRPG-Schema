using System.Collections.Generic;
using Project.Core.Events;
using Project.Data;
using Project.Gameplay.Economy;
using Project.Gameplay.Inventory;
using UnityEngine;

namespace Project.Gameplay.Trade
{
    /// <summary>
    /// MonoBehaviour per-instancja NPC-kupca, jak EnemyController - NIE IGameSystem. Przechowuje
    /// świeżą kopię zapasu ze ScriptableObject TraderData (nigdy nie mutuje samego assetu) i wykonuje
    /// transakcje kupna/sprzedaży. InventoryBridge/CurrencyBridge wskazują na GRACZA, nie na tego NPC -
    /// podpięte ręcznie w Inspectorze, tak jak inne mosty wymagające cross-referencji.
    ///
    /// CELOWO nie subskrybuje TradeRequestedEvent (mimo że mogłoby się to wydawać naturalnym miejscem) -
    /// patrz NpcDialogueInteractable.ActiveSpeaker i TradeUIController: ten komponent nie wie nic
    /// o dialogu/UI, to czysta logika handlu wywoływana z zewnątrz, dokładnie jak EnemyController.TakeDamage
    /// jest wołane z zewnątrz przez CombatSystem, a nie subskrybuje niczego samo.
    /// </summary>
    public class NpcMerchant : MonoBehaviour
    {
        [SerializeField] private TraderData traderData;
        [SerializeField] private InventoryBridge inventoryBridge;
        [SerializeField] private CurrencyBridge currencyBridge;

        private readonly List<TradeRuntimeSlot> _runtimeStock = new List<TradeRuntimeSlot>();

        public IReadOnlyList<TradeRuntimeSlot> Stock => _runtimeStock;

        private void Awake()
        {
            IReadOnlyList<TradeStockEntry> sourceStock = traderData.Stock;
            for (int i = 0; i < sourceStock.Count; i++)
            {
                TradeStockEntry entry = sourceStock[i];
                _runtimeStock.Add(new TradeRuntimeSlot
                {
                    Item = entry.Item,
                    Price = entry.Price,
                    Quantity = entry.StartingQuantity
                });
            }
        }

        /// <summary>
        /// All-or-nothing: jeśli TryAddItem się nie uda (ekwipunek gracza pełny), złoto wraca do
        /// gracza - spójne z EquipmentSystem.TryEquip/QuestSystem.TryTurnInQuest (rollback zamiast
        /// zostawiania gracza bez złota i bez przedmiotu).
        /// </summary>
        public bool TryBuy(int stockIndex)
        {
            if (stockIndex < 0 || stockIndex >= _runtimeStock.Count)
            {
                return false;
            }

            TradeRuntimeSlot slot = _runtimeStock[stockIndex];
            if (slot.Quantity <= 0)
            {
                return false;
            }

            if (!currencyBridge.Currency.TrySpendGold(slot.Price))
            {
                return false;
            }

            if (!inventoryBridge.Inventory.TryAddItem(slot.Item, 1))
            {
                // Rollback: ekwipunek gracza pełny - nie zabieraj mu złota za przedmiot, którego nie dostał.
                currencyBridge.Currency.AddGold(slot.Price);
                return false;
            }

            slot.Quantity -= 1;
            EventBus.Publish(new TradeStockChangedEvent());
            return true;
        }

        /// <summary>
        /// Sprzedany przedmiot NIE trafia do _runtimeStock tego NPC - świadome uproszczenie: NPC
        /// "pochłania" sprzedane przedmioty zamiast odsprzedawać je dalej, unikając komplikacji
        /// z pojemnością/duplikatami w zapasie NPC. BasePrice &lt;= 0 blokuje sprzedaż - domyślna
        /// wartość pola to 0, więc każdy ItemData bez jawnie ustawionej ceny (typowo: przedmioty
        /// questowe, ale też dowolny inny asset, któremu projektant nie nadał wartości) jest
        /// automatycznie niesprzedawalny, bez potrzeby osobnej flagi "IsSellable".
        /// </summary>
        public bool TrySell(ItemData item)
        {
            if (item == null || item.BasePrice <= 0)
            {
                return false;
            }

            if (inventoryBridge.Inventory.GetItemCount(item.ItemId) <= 0)
            {
                return false;
            }

            if (!inventoryBridge.Inventory.TryRemoveItem(item.ItemId, 1))
            {
                return false;
            }

            int sellPrice = Mathf.RoundToInt(item.BasePrice * traderData.SellMultiplier);
            currencyBridge.Currency.AddGold(sellPrice);
            EventBus.Publish(new TradeStockChangedEvent());
            return true;
        }
    }
}
