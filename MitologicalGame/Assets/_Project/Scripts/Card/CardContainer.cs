using System;
using System.Collections.Generic;
using _Project.Scripts.BaseAndInterfaces;
using _Project.Scripts.GameManagement;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Card
{
    public class CardContainer : MonoBehaviour
    {
        [SerializeField] public List<Transform> cardPositions;
        public Dictionary<int, bool> _cardStatus;
        public GameObject cardPrefab;
        private List<CardType> _requiredCardTypes;
        [FormerlySerializedAs("_availableCardTypes")] public List<CardType> availableCardTypes;

        private void Start()
        {
            InitializeContainer();
            PrepareRequiredCards();
            OrderCards();
        }

        private void InitializeContainer()
        {
            _cardStatus = new Dictionary<int, bool>();
            _requiredCardTypes = new List<CardType>();
            availableCardTypes = new List<CardType>();
        
            if (cardPositions.Count == 0) return;
        
            for (int i = 0; i < cardPositions.Count; i++)
            {
                _cardStatus.Add(i, true);
            }
        }

        public void CheckForEmptyPositions()
        {
            bool hasEmptyPositions = false;
            int emptyCount = 0;
        
            for (int i = 0; i < cardPositions.Count; i++)
            {
                bool isEmpty = cardPositions[i].childCount == 0;
            
                if (isEmpty && !_cardStatus[i])
                {
                    _cardStatus[i] = true;
                    hasEmptyPositions = true;
                    emptyCount++;
                }
                else if (!isEmpty && _cardStatus[i])
                {
                    _cardStatus[i] = false;
                }
            }

            if (hasEmptyPositions)
            {
                if (availableCardTypes.Count < emptyCount)
                {
                    PrepareRequiredCards();
                }
                else
                {
                    ShuffleCards(availableCardTypes);
                }
            
                OrderCards();
            }
        }

        public void OrderCards()
        {
            int currentCardIndex = 0;
        
            for (int i = 0; i < _cardStatus.Count; i++)
            {
                if (_cardStatus[i] && cardPositions[i].childCount == 0 && currentCardIndex < availableCardTypes.Count)
                {
                    GameObject card = Instantiate(cardPrefab, cardPositions[i], true);
                    card.name = "Card " + i;
                    card.transform.position = cardPositions[i].position;

                    var cardBehavior = card.GetComponent<CardBehaviours>();
                    if (cardBehavior != null)
                    {
                        cardBehavior.SetCardType(availableCardTypes[currentCardIndex]);
                        cardBehavior.Initialize();
                    }

                    _cardStatus[i] = false;
                    currentCardIndex++;
                }
            }
        
            if (currentCardIndex > 0)
            {
                availableCardTypes.RemoveRange(0, currentCardIndex);
            }
        
            if (availableCardTypes.Count == 0)
            {
                AddCardFromDiscard();
            }
        }

        private void PrepareRequiredCards()
        {
            _requiredCardTypes.Clear();
            availableCardTypes.Clear();

            var cardTypes = Enum.GetValues(typeof(CardType));
        
            foreach (CardType type in cardTypes)
            {
                _requiredCardTypes.Add(type);
            }

            int remainingPositions = cardPositions.Count - _requiredCardTypes.Count;
            for (int i = 0; i < remainingPositions; i++)
            {
                CardType randomType = (CardType)UnityEngine.Random.Range(0, cardTypes.Length);
                _requiredCardTypes.Add(randomType);
            }

            ShuffleCards(_requiredCardTypes);
            availableCardTypes.AddRange(_requiredCardTypes);
        }

        private void ShuffleCards(List<CardType> cards)
        {
            // Fisher-Yates shuffle algoritması
            int n = cards.Count;
            while (n > 1)
            {
                n--;
                int k = UnityEngine.Random.Range(0, n + 1);
                (cards[k], cards[n]) = (cards[n], cards[k]);
            }
        }
        private void AddCardFromDiscard()
        {
            Debug.LogWarning("All cards are used. Adding cards from discarded cards.");
            var discardedCards = NewGameManager.Instance.discardedCards;
        
            if (discardedCards.Count > 0)
            {
                availableCardTypes.Clear();
                for (int i = discardedCards.Count - 1; i >= 0; i--)
                {
                    availableCardTypes.Add(discardedCards[i]);
                    discardedCards.RemoveAt(i);
                }
            
                ShuffleCards(availableCardTypes);
            }
            else
            {
                PrepareRequiredCards();
            }
        }
    }
}