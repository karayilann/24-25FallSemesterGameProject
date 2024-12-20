using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.BaseAndInterfaces;
using _Project.Scripts.GameManagement;
using UnityEngine;

namespace _Project.Scripts.Card
{
    public class CardContainer : MonoBehaviour
    {
        [SerializeField] public List<Transform> cardPositions;
        public Dictionary<int, bool> _cardStatus;
        public GameObject cardPrefab;
        public List<CardType> _requiredCardTypes;
        private int _totalCardCount = 5;
        private int _requiredCardCount = 0;

        private void Start()
        {
            _cardStatus = new Dictionary<int, bool>();
            _requiredCardTypes = new List<CardType>();
            
            if (cardPositions.Count == 0) return;
            
            for (int i = 0; i < cardPositions.Count; i++)
            {
                _cardStatus.Add(i, true);
            }

            PrepareRequiredCards();
            OrderCards();
        }

        private void PrepareRequiredCards()
        {
            foreach (CardType type in Enum.GetValues(typeof(CardType)))
            {
                _requiredCardTypes.Add(type);
            }

            int remainingPositions = cardPositions.Count - _requiredCardTypes.Count;
            for (int i = 0; i < remainingPositions; i++)
            {
                CardType randomType = (CardType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(CardType)).Length);
                _requiredCardTypes.Add(randomType);
            }

            _requiredCardTypes = _requiredCardTypes.OrderBy(x => UnityEngine.Random.value).ToList();
        }

        public void OrderCards()
        {
            for (int i = 0; i < _cardStatus.Count; i++)
            {
                if (_cardStatus[i] && i < _requiredCardTypes.Count)
                {
                    GameObject card = Instantiate(cardPrefab, cardPositions[i], true);
                    card.name = "Card " + i;
                    card.transform.position = cardPositions[i].position;

                    var cardBehavior = card.GetComponent<CardBehaviours>();
                    if (cardBehavior != null)
                    {
                        cardBehavior.SetCardType(_requiredCardTypes[i]);
                        cardBehavior.Initialize();
                    }

                    _cardStatus[i] = false;
                    //_totalCardCount--;
                }
            }

            //_requiredCardCount = 0;
        }

        private void AddCardFromDiscard()
        {
            Debug.LogWarning("All cards are used. Adding cards from discarded cards.");
            _requiredCardTypes.Clear();
            var discardedCards = NewGameManager.Instance.discardedCards;
            _totalCardCount = discardedCards.Count;
        
            // ToList() kullanımını kaldır ve doğrudan listeyi işle
            for (int i = discardedCards.Count - 1; i >= 0; i--)
            {
                _requiredCardTypes.Add(discardedCards[i]);
                discardedCards.RemoveAt(i);
            }
        }

        public void CheckForEmptyPositions()
        {
            for (int i = 0; i < cardPositions.Count; i++)
            {
                if (cardPositions[i].childCount == 0)
                {
                    _cardStatus[i] = true;
                    _requiredCardCount++;
                }
            }
            OrderCards();
        }
        
    }
}