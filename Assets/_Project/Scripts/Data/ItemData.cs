using UnityEngine;

namespace Project.Data
{
    /// <summary>
    /// Kategorie przedmiotów. Wartości int przypisane jawnie: Weapon/Armor/Material/Quest
    /// zachowują swoje dotychczasowe pozycje (0/1/4/3, dawniej QuestItem), żeby nie zerwać
    /// już zapisanych referencji w istniejących ItemData assetach (np. IronSword.asset).
    /// Consumable(2)/Misc(5) zostały zastąpione nowymi kategoriami - assety, które miały
    /// przypisaną jedną z tych dwóch wartości, będą wymagały ręcznej korekty w Inspectorze.
    /// </summary>
    public enum ItemType
    {
        Weapon = 0,
        Armor = 1,
        Jewelry = 2,
        Potion = 5,
        Book = 6,
        Magic = 7,
        Food = 8,
        Material = 4,
        Quest = 3
    }

    /// <summary>
    /// Statyczna definicja przedmiotu w grze. Instancje tworzone jako assety przez
    /// menu Gothic/Item, referencjonowane z Inventory/Interaction, nigdy nie modyfikowane
    /// w runtime (stan posiadanej ilości trzyma <see cref="Project.Gameplay.Inventory.InventorySlot"/>).
    /// </summary>
    [CreateAssetMenu(fileName = "NewItem", menuName = "Gothic/Item")]
    public class ItemData : ScriptableObject
    {
        [SerializeField] private string itemId;
        [SerializeField] private string displayName;
        [TextArea]
        [SerializeField] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField] private bool isStackable;
        [SerializeField] private int maxStackSize = 1;
        [SerializeField] private ItemType itemType;
        [SerializeField] private float damageValue;
        [SerializeField] private float armorValue;

        /// <summary>Unikalny identyfikator przedmiotu (np. "iron_sword") - niezależny od nazwy assetu.</summary>
        public string ItemId => itemId;

        public string DisplayName => displayName;

        public string Description => description;

        public Sprite Icon => icon;

        public bool IsStackable => isStackable;

        /// <summary>Ignorowane, jeśli IsStackable == false.</summary>
        public int MaxStackSize => maxStackSize;

        public ItemType ItemType => itemType;

        /// <summary>Sensowne tylko dla ItemType.Weapon, ignorowane dla innych typów - CombatBridge
        /// odczytuje tę wartość dla EquippedWeapon niezależnie od ItemType, więc pole celowo nie ma
        /// walidacji/enforce w edytorze na tym etapie.</summary>
        public float DamageValue => damageValue;

        /// <summary>Sensowne tylko dla ItemType.Armor, ignorowane dla innych typów - tak samo luźne
        /// jak DamageValue, bez walidacji/enforce w edytorze.</summary>
        public float ArmorValue => armorValue;
    }
}
