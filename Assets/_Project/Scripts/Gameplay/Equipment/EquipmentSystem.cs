using Project.Core.Events;
using Project.Core.Systems;
using Project.Data;
using Project.Gameplay.Inventory;

namespace Project.Gameplay.Equipment
{
    /// <summary>
    /// Czysta klasa C# zarządzająca pięcioma slotami ekwipunku zakładanego (broń/zbroja/2 pierścienie/
    /// naszyjnik). TryEquip sam wylicza docelowy EquipmentSlot z ItemType/JewelryType przedmiotu -
    /// wywołujący (UI) nie musi wiedzieć nic o slotach, po prostu przekazuje ItemData, tak jak
    /// dotychczas. Pierścień trafia do pierwszego wolnego slotu (Ring1 potem Ring2); jeśli oba zajęte,
    /// zastępuje Ring1 - świadomie, zamiast wymuszać na graczu wybór konkretnego slotu (może zdjąć
    /// niechciany pierścień ręcznie, jeśli automatyczny wybór mu nie odpowiada).
    /// </summary>
    public class EquipmentSystem : IGameSystem
    {
        private readonly InventorySystem _inventorySystem;

        public ItemData EquippedWeapon { get; private set; }
        public ItemData EquippedArmor { get; private set; }
        public ItemData EquippedRing1 { get; private set; }
        public ItemData EquippedRing2 { get; private set; }
        public ItemData EquippedNecklace { get; private set; }

        public EquipmentSystem(InventorySystem inventorySystem)
        {
            _inventorySystem = inventorySystem;
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

        public bool TryEquip(ItemData item)
        {
            if (item == null)
            {
                return false;
            }

            EquipmentSlot? targetSlot = ResolveTargetSlot(item);
            if (targetSlot == null)
            {
                return false;
            }

            if (!_inventorySystem.TryRemoveItem(item.ItemId, 1))
            {
                return false;
            }

            ItemData previousItem = GetEquippedItem(targetSlot.Value);
            if (previousItem != null && !_inventorySystem.TryAddItem(previousItem, 1))
            {
                // Rollback: nie ma miejsca na stary przedmiot - nie zdejmuj nowego z ekwipunku.
                _inventorySystem.TryAddItem(item, 1);
                return false;
            }

            SetEquippedItem(targetSlot.Value, item);
            EventBus.Publish(new EquipmentChangedEvent());
            return true;
        }

        public bool TryUnequip(EquipmentSlot slot)
        {
            ItemData equippedItem = GetEquippedItem(slot);
            if (equippedItem == null)
            {
                return false;
            }

            if (!_inventorySystem.TryAddItem(equippedItem, 1))
            {
                return false;
            }

            SetEquippedItem(slot, null);
            EventBus.Publish(new EquipmentChangedEvent());
            return true;
        }

        private EquipmentSlot? ResolveTargetSlot(ItemData item)
        {
            switch (item.ItemType)
            {
                case ItemType.Weapon:
                    return EquipmentSlot.Weapon;
                case ItemType.Armor:
                    return EquipmentSlot.Armor;
                case ItemType.Jewelry:
                    return ResolveJewelrySlot(item);
                default:
                    return null;
            }
        }

        private EquipmentSlot ResolveJewelrySlot(ItemData item)
        {
            if (item.JewelryType == JewelryType.Necklace)
            {
                return EquipmentSlot.Necklace;
            }

            if (EquippedRing1 == null)
            {
                return EquipmentSlot.Ring1;
            }

            if (EquippedRing2 == null)
            {
                return EquipmentSlot.Ring2;
            }

            return EquipmentSlot.Ring1;
        }

        private ItemData GetEquippedItem(EquipmentSlot slot)
        {
            switch (slot)
            {
                case EquipmentSlot.Weapon:
                    return EquippedWeapon;
                case EquipmentSlot.Armor:
                    return EquippedArmor;
                case EquipmentSlot.Ring1:
                    return EquippedRing1;
                case EquipmentSlot.Ring2:
                    return EquippedRing2;
                case EquipmentSlot.Necklace:
                    return EquippedNecklace;
                default:
                    return null;
            }
        }

        private void SetEquippedItem(EquipmentSlot slot, ItemData item)
        {
            switch (slot)
            {
                case EquipmentSlot.Weapon:
                    EquippedWeapon = item;
                    break;
                case EquipmentSlot.Armor:
                    EquippedArmor = item;
                    break;
                case EquipmentSlot.Ring1:
                    EquippedRing1 = item;
                    break;
                case EquipmentSlot.Ring2:
                    EquippedRing2 = item;
                    break;
                case EquipmentSlot.Necklace:
                    EquippedNecklace = item;
                    break;
            }
        }
    }
}
