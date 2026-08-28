using System;
using UnityEngine;

namespace Project.Data
{
    /// <summary>
    /// Nagroda za oddanie questa. Zwykła serializowalna klasa (nie ScriptableObject) -
    /// zagnieżdżona wewnątrz QuestData.
    /// </summary>
    [Serializable]
    public class QuestReward
    {
        [SerializeField] private int goldAmount = 0;
        [SerializeField] private ItemData rewardItem;
        [SerializeField] private int rewardItemQuantity = 1;
        [SerializeField] private int xpAmount = 0;

        public int GoldAmount => goldAmount;

        /// <summary>Opcjonalny przedmiot nagrody. null = brak przedmiotu (tylko złoto, jeśli > 0).</summary>
        public ItemData RewardItem => rewardItem;

        /// <summary>Ignorowane, jeśli RewardItem == null.</summary>
        public int RewardItemQuantity => rewardItemQuantity;

        public int XpAmount => xpAmount;
    }
}
