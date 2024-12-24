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

        private const int TotalCardCount = 25; // Toplam kart sayısı (5 kart türünden her biri 5 adet)

        private void Start()
        {
            InitializeContainer();
            PrepareCardDeck(); // Tüm kartlardan 5 tane olacak şekilde hazırlık
            OrderCards();
        }

        private void InitializeContainer()
        {
            _cardStatus = new Dictionary<int, bool>();

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
                ShuffleCards(availableCardTypes); // Desteden rastgele kart seçimi için karıştırma
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
                Debug.LogWarning("All cards have been used. No more cards in the deck.");
                AddCardFromDiscardPile();
            }
        }

        private void AddCardFromDiscardPile()
        {
            var newGameManager = NewGameManager.Instance;
            if (newGameManager.discardedCards.Count == 0)
            {
                Debug.LogWarning("No discarded cards to add.");
                return;
            }

            foreach (var card in newGameManager.discardedCards)
            {
                card.dragAndDrop.cardCollider.enabled = true;
                card.dragAndDrop.canDrag = true;
                card.dragAndDrop.isProccessed = false;
                availableCardTypes.Add(card.CardType);
            }
            
            ShuffleCards(availableCardTypes);
            if(newGameManager.discardedCards.Count != 0) return;
            NewGameManager.Instance.discardedCards.Clear();
        }

        private void PrepareCardDeck()
        {
            availableCardTypes = new List<CardType>();

            var cardTypes = Enum.GetValues(typeof(CardType));

            foreach (CardType type in cardTypes)
            {
                for (int i = 0; i < 5; i++)
                {
                    availableCardTypes.Add(type);
                }
            }

            if (availableCardTypes.Count != TotalCardCount)
            {
                Debug.LogError("Card deck setup error: Incorrect total card count.");
            }

            ShuffleCards(availableCardTypes);
        }

        private void ShuffleCards(List<CardType> cards)
        {
            int n = cards.Count;
            while (n > 1)
            {
                n--;
                int k = UnityEngine.Random.Range(0, n + 1);
                (cards[k], cards[n]) = (cards[n], cards[k]);
            }
        }
    }
}
