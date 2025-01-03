using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _Project.Scripts.BaseAndInterfaces;
using _Project.Scripts.GameManagement;
using Random = UnityEngine.Random;

namespace _Project.Scripts.Card
{
    public class CardContainer : MonoBehaviour
    {
        [SerializeField] public List<Transform> cardPositions;
        public Dictionary<int, bool> _cardStatus;
        [SerializeField] private List<GameObject> cardPrefabs; // Card prefabs instead of types
        private List<GameObject> _availableCards; // Available cards to spawn
        
        [SerializeField] private int closedCardsCount = 0;
        private List<int> _closedCardPositions = new List<int>();
        private const int TotalCardCount = 25;
        private NewGameManager _newGameManager;
        
        private readonly Vector3 _closedRotation = new Vector3(0, 90, 0);
        private readonly Vector3 _openRotation = new Vector3(0, -90, 0);
        
        private void Start()
        {
            InitializeContainer();
            PrepareCardDeck();
            OrderCards();
            _newGameManager = NewGameManager.Instance;
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

        public void SetClosedCardsCount(int count)
        {
            closedCardsCount = Mathf.Min(count, cardPositions.Count);
            _closedCardPositions.Clear();
        }
        
        public void SetClosedCardsCountBasedOnChance(int chancePercentage)
        {
            int maxClosedCards = Mathf.CeilToInt((chancePercentage / 100f) * cardPositions.Count);
            closedCardsCount = Mathf.Min(maxClosedCards, cardPositions.Count);
            _closedCardPositions.Clear();
        }

        public void CheckForEmptyPositions(int chancePercentage = 0)
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
                int closedCardChance = _newGameManager.GetClosedCardChance();
                int totalCards = cardPositions.Count;
                int closedCardCount = Mathf.RoundToInt(totalCards * (closedCardChance / 100f));

                SetClosedCardsCount(closedCardCount);
                ShuffleCards(_availableCards);
                OrderCards();
            }
        }

        public void OrderCards()
        {
            int currentCardIndex = 0;
            _closedCardPositions.Clear();

            List<int> availablePositions = new List<int>();
            for (int i = 0; i < _cardStatus.Count; i++)
            {
                if (_cardStatus[i]) availablePositions.Add(i);
            }

            for (int i = 0; i < closedCardsCount && availablePositions.Count > 0; i++)
            {
                int randomIndex = Random.Range(0, availablePositions.Count);
                _closedCardPositions.Add(availablePositions[randomIndex]);
                availablePositions.RemoveAt(randomIndex);
            }

            for (int i = 0; i < _cardStatus.Count; i++)
            {
                if (_cardStatus[i] && cardPositions[i].childCount == 0 && currentCardIndex < _availableCards.Count)
                {
                    GameObject card = Instantiate(_availableCards[currentCardIndex], cardPositions[i], true);
                    card.name = "Card " + i;
                    card.transform.position = cardPositions[i].position;

                    var cardBehavior = card.GetComponent<CardBehaviours>();
                    if (cardBehavior != null)
                    {
                        cardBehavior.Initialize();
                        bool shouldBeClosed = _closedCardPositions.Contains(i);
                        SetupCardState(card, cardBehavior, shouldBeClosed);
                    }

                    _cardStatus[i] = false;
                    currentCardIndex++;
                }
            }

            if (currentCardIndex > 0)
            {
                _availableCards.RemoveRange(0, currentCardIndex);
            }

            if (_availableCards.Count == 0)
            {
                Debug.LogWarning("All cards have been used. No more cards in the deck.");
                AddCardFromDiscardPile();
            }
        }

        private void SetupCardState(GameObject cardObject, CardBehaviours cardBehavior, bool isClosed)
        {
            var dragAndDrop = cardObject.GetComponent<DragAndDrop>();
            if (dragAndDrop != null)
            {
                dragAndDrop.canDrag = !isClosed;
            }

            if (isClosed)
            {
                cardObject.transform.rotation = Quaternion.Euler(_closedRotation);
                cardBehavior.CurrentStatus = CardStatus.Closed;
            }
            else
            {
                cardObject.transform.rotation = Quaternion.Euler(_openRotation);
                cardBehavior.CurrentStatus = CardStatus.Opened;
            }
        }

        public void OnCardClicked(GameObject cardObject)
        {
            if(_newGameManager.ForesightCount == 0) return;
            var cardBehavior = cardObject.GetComponent<CardBehaviours>();
            var dragAndDrop = cardBehavior.dragAndDrop;

            if (cardBehavior != null && cardBehavior.CurrentStatus == CardStatus.Closed)
            {
                StartCoroutine(FlipCard(cardObject, cardBehavior, dragAndDrop));
            }
        }

        private IEnumerator FlipCard(GameObject cardObject, CardBehaviours cardBehavior, DragAndDrop dragAndDrop)
        {
            float duration = 0.5f;
            float elapsed = 0f;
            Quaternion startRotation = Quaternion.Euler(_closedRotation);
            Quaternion targetRotation = Quaternion.Euler(_openRotation);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                cardObject.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
                yield return null;
            }

            cardObject.transform.rotation = targetRotation;
            cardBehavior.CurrentStatus = CardStatus.Opened;
            if (dragAndDrop != null)
            {
                dragAndDrop.canDrag = true;
                _newGameManager.ChangeForesightCount(-1);
            }
        }

        private void AddCardFromDiscardPile()
        {
            if (_newGameManager.discardedCards.Count == 0)
            {
                Debug.LogWarning("No discarded cards to add.");
                return;
            }

            foreach (var cardBehavior in _newGameManager.discardedCards)
            {
                var cardPrefab = cardPrefabs.Find(p => p.GetComponent<CardBehaviours>().CardType == cardBehavior.CardType);
                if (cardPrefab != null)
                {
                    _availableCards.Add(cardPrefab);
                }
            }
            
            ShuffleCards(_availableCards);
            if(_newGameManager.discardedCards.Count != 0) return;
            _newGameManager.discardedCards.Clear();
        }

        private void PrepareCardDeck()
        {
            _availableCards = new List<GameObject>();

            foreach (var cardPrefab in cardPrefabs)
            {
                for (int i = 0; i < 5; i++)
                {
                    _availableCards.Add(cardPrefab);
                }
            }

            if (_availableCards.Count != TotalCardCount)
            {
                Debug.LogError($"Card deck setup error: Incorrect total card count. Expected {TotalCardCount}, got {_availableCards.Count}");
            }

            ShuffleCards(_availableCards);
        }

        private void ShuffleCards(List<GameObject> cards)
        {
            int n = cards.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                (cards[k], cards[n]) = (cards[n], cards[k]);
            }
        }
    }
}