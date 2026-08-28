using System.Collections.Generic;
using Project.Gameplay.Stats;
using UnityEngine;

namespace Project.Data
{
    /// <summary>Pojedyncza oferta nauki u konkretnego nauczyciela - jeden atrybut, jego koszt
    /// i limit. Analogicznie do TradeStockEntry w TraderData.</summary>
    [System.Serializable]
    public class TrainingOffer
    {
        [SerializeField] private AttributeType attribute;
        [SerializeField] private int amount = 1;
        [SerializeField] private int learningPointCost = 1;
        [SerializeField] private int goldCost = 0;
        [SerializeField] private int maxAttributeValue = 100;

        public AttributeType Attribute => attribute;
        /// <summary>Typowo 1 lub 5 - o ile atrybut wzrasta za jedno szkolenie.</summary>
        public int Amount => amount;
        public int LearningPointCost => learningPointCost;
        public int GoldCost => goldCost;
        /// <summary>Ten nauczyciel odmówi nauki, jeśli atrybut gracza jest już >= tej wartości -
        /// klasyczny limit nauczyciela z Gothic (różni nauczyciele uczą do różnego poziomu).</summary>
        public int MaxAttributeValue => maxAttributeValue;
    }

    /// <summary>Statyczna definicja oferty nauczyciela. Instancje jako assety przez menu
    /// Gothic/Trainer, referencjonowane z NpcTrainer, nigdy nie modyfikowane w runtime.</summary>
    [CreateAssetMenu(fileName = "NewTrainer", menuName = "Gothic/Trainer")]
    public class TrainerData : ScriptableObject
    {
        [SerializeField] private List<TrainingOffer> offers = new List<TrainingOffer>();
        public IReadOnlyList<TrainingOffer> Offers => offers;
    }
}
