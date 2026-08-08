using System;
using UnityEngine;

namespace Project.Data
{
    /// <summary>
    /// Pojedyncza pozycja towaru handlarza. Zwykła serializowalna klasa (nie ScriptableObject) -
    /// zagnieżdżona w liście wewnątrz TraderData, analogicznie do DialogueOption w DialogueNode.
    /// Price jest niezależna od ItemData.BasePrice - to cena ZAKUPU u TEGO KONKRETNEGO NPC, ustalana
    /// ręcznie przez projektanta per-handlarz (dwóch różnych kupców może sprzedawać ten sam miecz
    /// za różną cenę).
    /// </summary>
    [Serializable]
    public class TradeStockEntry
    {
        [SerializeField] private ItemData item;
        [SerializeField] private int price;
        [SerializeField] private int startingQuantity;

        public ItemData Item => item;

        /// <summary>Cena zakupu u tego NPC - niezależna od ItemData.BasePrice.</summary>
        public int Price => price;

        public int StartingQuantity => startingQuantity;
    }
}
