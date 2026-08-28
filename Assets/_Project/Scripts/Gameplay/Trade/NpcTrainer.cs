using System.Collections.Generic;
using Project.Core.Events;
using Project.Data;
using Project.Gameplay.Dialogue;
using Project.Gameplay.Economy;
using Project.Gameplay.Stats;
using UnityEngine;

namespace Project.Gameplay.Trade
{
    /// <summary>
    /// MonoBehaviour per-instancja NPC-nauczyciela, analogicznie do NpcMerchant. W PRZECIWIEŃSTWIE
    /// do NpcMerchant (który celowo nie subskrybuje TradeRequestedEvent, bo pośredniczy w tym
    /// TradeUIController pokazujący panel), NpcTrainer subskrybuje TrainingRequestedEvent
    /// BEZPOŚREDNIO - nie ma tu żadnego UI do otworzenia, cała transakcja (koszt LP+złoto, wzrost
    /// atrybutu) dzieje się natychmiast przy wyborze opcji dialogowej. Filtrowanie przez
    /// ActiveSpeaker zapobiega reakcji WSZYSTKICH nauczycieli w scenie na jeden event.
    /// </summary>
    public class NpcTrainer : MonoBehaviour
    {
        [SerializeField] private TrainerData trainerData;
        [SerializeField] private PlayerStatsBridge playerStatsBridge;
        [SerializeField] private CurrencyBridge currencyBridge;

        private void OnEnable()
        {
            EventBus.Subscribe<TrainingRequestedEvent>(OnTrainingRequested);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<TrainingRequestedEvent>(OnTrainingRequested);
        }

        private void OnTrainingRequested(TrainingRequestedEvent evt)
        {
            if (NpcDialogueInteractable.ActiveSpeaker != gameObject)
            {
                return;
            }

            TryTrain(evt.Attribute);
        }

        /// <summary>
        /// Wszystkie warunki sprawdzane PRZED jakąkolwiek zmianą stanu (brak potrzeby rollbacku,
        /// w przeciwieństwie do TryEquip/TryBuy) - dopiero gdy limit/LP/złoto wszystkie się zgadzają,
        /// faktycznie odejmuje koszt i podnosi atrybut.
        /// </summary>
        public bool TryTrain(AttributeType attribute)
        {
            TrainingOffer offer = FindOffer(attribute);
            if (offer == null)
            {
                return false;
            }

            int currentValue = playerStatsBridge.Stats.GetAttribute(attribute);
            if (currentValue >= offer.MaxAttributeValue)
            {
                Debug.Log("NpcTrainer: gracz osiągnął już limit tego nauczyciela dla " + attribute);
                return false;
            }

            if (playerStatsBridge.Stats.LearningPoints < offer.LearningPointCost)
            {
                Debug.Log("NpcTrainer: gracz nie ma wystarczająco Punktów Nauki.");
                return false;
            }

            if (currencyBridge.Currency.Gold < offer.GoldCost)
            {
                Debug.Log("NpcTrainer: gracz nie ma wystarczająco złota.");
                return false;
            }

            playerStatsBridge.Stats.TrySpendLearningPoints(offer.LearningPointCost);
            currencyBridge.Currency.TrySpendGold(offer.GoldCost);
            playerStatsBridge.Stats.SetAttribute(attribute, currentValue + offer.Amount);
            return true;
        }

        private TrainingOffer FindOffer(AttributeType attribute)
        {
            IReadOnlyList<TrainingOffer> offers = trainerData.Offers;
            for (int i = 0; i < offers.Count; i++)
            {
                if (offers[i].Attribute == attribute)
                {
                    return offers[i];
                }
            }

            return null;
        }
    }
}
