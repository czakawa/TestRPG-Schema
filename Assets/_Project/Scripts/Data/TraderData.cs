using System.Collections.Generic;
using UnityEngine;

namespace Project.Data
{
    /// <summary>
    /// Statyczna definicja oferty handlarza. Instancje tworzone jako assety przez menu Gothic/Trader,
    /// referencjonowane z NpcMerchant, nigdy nie modyfikowane w runtime (stan bieżącego zapasu
    /// per-NPC trzyma jego własna kopia w <see cref="Project.Gameplay.Trade.TradeRuntimeSlot"/>).
    /// </summary>
    [CreateAssetMenu(fileName = "NewTrader", menuName = "Gothic/Trader")]
    public class TraderData : ScriptableObject
    {
        [SerializeField] private List<TradeStockEntry> stock = new List<TradeStockEntry>();
        [SerializeField] private float sellMultiplier = 0.5f;

        public IReadOnlyList<TradeStockEntry> Stock => stock;

        /// <summary>Mnożnik ItemData.BasePrice przy sprzedaży NPC (NpcMerchant.TrySell).</summary>
        public float SellMultiplier => sellMultiplier;
    }
}
