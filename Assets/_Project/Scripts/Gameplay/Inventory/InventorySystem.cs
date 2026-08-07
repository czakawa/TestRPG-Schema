using System.Collections.Generic;
using Project.Core.Events;
using Project.Core.Systems;
using Project.Data;

namespace Project.Gameplay.Inventory
{
    /// <summary>
    /// Czysta klasa C# (nie MonoBehaviour) zarządzająca stanem ekwipunku o stałej pojemności.
    /// Sloty są prealokowane w Initialize i mutowane in-place (Item/Quantity) - dodawanie
    /// i usuwanie przedmiotów nigdy nie tworzy nowej listy ani nowych obiektów slotów.
    /// </summary>
    public class InventorySystem : IGameSystem
    {
        private readonly int _capacity;
        private readonly List<InventorySlot> _slots;

        public IReadOnlyList<InventorySlot> Slots => _slots;

        public InventorySystem(int capacity)
        {
            _capacity = capacity;
            _slots = new List<InventorySlot>(capacity);
        }

        public void Initialize()
        {
            _slots.Clear();
            for (int i = 0; i < _capacity; i++)
            {
                _slots.Add(new InventorySlot());
            }
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
        /// Dodaje przedmiot do ekwipunku: najpierw dokłada do istniejących stacków tego samego itemId
        /// (jeśli isStackable), nadmiar trafia do nowych pustych slotów. All-or-nothing - jeśli nie ma
        /// wystarczająco miejsca na całą ilość, nic nie jest dodawane i metoda zwraca false.
        /// </summary>
        public bool TryAddItem(ItemData item, int quantity = 1)
        {
            if (item == null || quantity <= 0)
            {
                return false;
            }

            if (!HasCapacityFor(item, quantity))
            {
                return false;
            }

            int remaining = quantity;

            if (item.IsStackable)
            {
                int count = _slots.Count;
                for (int i = 0; i < count && remaining > 0; i++)
                {
                    InventorySlot slot = _slots[i];
                    if (slot.Item != item)
                    {
                        continue;
                    }

                    int freeSpace = item.MaxStackSize - slot.Quantity;
                    if (freeSpace <= 0)
                    {
                        continue;
                    }

                    int amountToAdd = remaining < freeSpace ? remaining : freeSpace;
                    slot.Quantity += amountToAdd;
                    remaining -= amountToAdd;
                }
            }

            int stackSize = item.IsStackable ? item.MaxStackSize : 1;
            while (remaining > 0)
            {
                InventorySlot emptySlot = FindEmptySlot();
                int amountToAdd = remaining < stackSize ? remaining : stackSize;

                emptySlot.Item = item;
                emptySlot.Quantity = amountToAdd;
                remaining -= amountToAdd;
            }

            EventBus.Publish(new InventoryChangedEvent());
            return true;
        }

        /// <summary>Usuwa daną ilość przedmiotu o podanym itemId, zaczynając od pierwszych pasujących slotów.</summary>
        public bool TryRemoveItem(string itemId, int quantity = 1)
        {
            if (string.IsNullOrEmpty(itemId) || quantity <= 0)
            {
                return false;
            }

            if (GetItemCount(itemId) < quantity)
            {
                return false;
            }

            int remaining = quantity;
            int count = _slots.Count;
            for (int i = 0; i < count && remaining > 0; i++)
            {
                InventorySlot slot = _slots[i];
                if (slot.IsEmpty || slot.Item.ItemId != itemId)
                {
                    continue;
                }

                int amountToRemove = remaining < slot.Quantity ? remaining : slot.Quantity;
                slot.Quantity -= amountToRemove;
                remaining -= amountToRemove;

                if (slot.Quantity <= 0)
                {
                    slot.Clear();
                }
            }

            EventBus.Publish(new InventoryChangedEvent());
            return true;
        }

        /// <summary>Sumuje ilość posiadanego przedmiotu o danym itemId po wszystkich slotach.</summary>
        public int GetItemCount(string itemId)
        {
            int total = 0;
            int count = _slots.Count;
            for (int i = 0; i < count; i++)
            {
                InventorySlot slot = _slots[i];
                if (!slot.IsEmpty && slot.Item.ItemId == itemId)
                {
                    total += slot.Quantity;
                }
            }

            return total;
        }

        private bool HasCapacityFor(ItemData item, int quantity)
        {
            int remaining = quantity;
            int count = _slots.Count;

            if (item.IsStackable)
            {
                for (int i = 0; i < count && remaining > 0; i++)
                {
                    InventorySlot slot = _slots[i];
                    if (slot.Item == item)
                    {
                        remaining -= item.MaxStackSize - slot.Quantity;
                    }
                }
            }

            if (remaining <= 0)
            {
                return true;
            }

            int stackSize = item.IsStackable ? item.MaxStackSize : 1;
            int emptySlotsNeeded = (remaining + stackSize - 1) / stackSize;

            for (int i = 0; i < count; i++)
            {
                if (_slots[i].IsEmpty)
                {
                    emptySlotsNeeded--;
                    if (emptySlotsNeeded <= 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private InventorySlot FindEmptySlot()
        {
            int count = _slots.Count;
            for (int i = 0; i < count; i++)
            {
                if (_slots[i].IsEmpty)
                {
                    return _slots[i];
                }
            }

            return null;
        }
    }
}
