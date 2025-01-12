using System;
using System.Collections.Generic;
using _Project.Scripts._2DCardScripts;
using UnityEngine;
using _Project.Scripts.BaseAndInterfaces;
using _Project.Scripts.GameManagement;
using DG.Tweening;
using Random = UnityEngine.Random;

namespace _Project.Scripts.Card
{
    public class CardContainer : MonoBehaviour
    {
        #region Variables
        [Header("Card Settings")]
        [SerializeField] public List<Transform> cardPositions;
        [SerializeField] private List<GameObject> cardPrefabs;
        [SerializeField] private int closedCardsCount = 0;

        [Header("Animation Settings")]
        [SerializeField] private float cardFlipDuration = 0.5f;
        [SerializeField] private float cardSpawnDuration = 0.3f;
        [SerializeField] private float cardMoveDuration = 0.3f;
        [SerializeField] private float spawnDelay = 0.1f;
        [SerializeField] private Ease cardFlipEase = Ease.InOutQuad;
        [SerializeField] private Ease cardSpawnEase = Ease.OutBack;
        [SerializeField] private Ease cardMoveEase = Ease.OutQuint;

        private Dictionary<int, bool> _cardStatus;
        private List<GameObject> _availableCards;
        private List<int> _closedCardPositions;
        private GameManager2D _newGameManager;
        
        private const int TotalCardCount = 25;
        private const int CardsPerType = 5;
        
        private readonly Vector3 _closedRotation = new Vector3(0, 90, 0);
        private readonly Vector3 _openRotation = Vector3.zero;
        private readonly Vector3 _cardSpawnScale = Vector3.zero;
        #endregion

        #region Unity Methods
        private void Awake()
        {
            InitializeContainer();
            _newGameManager = GameManager2D.Instance;
        }

        private void Start()
        {
            PrepareCardDeck();
            OrderCards();
        }

        private void OnDestroy()
        {
            DOTween.Kill(transform);
            _cardStatus?.Clear();
            _availableCards?.Clear();
            _closedCardPositions?.Clear();
        }
        #endregion

        #region Initialization
        private void InitializeContainer()
        {
            _cardStatus = new Dictionary<int, bool>();
            _closedCardPositions = new List<int>();
            
            if (cardPositions == null || cardPositions.Count == 0)
            {
                Debug.LogError("Card positions not set!");
                return;
            }

            for (int i = 0; i < cardPositions.Count; i++)
            {
                _cardStatus.Add(i, true);
            }
        }

        private void PrepareCardDeck()
        {
            _availableCards = new List<GameObject>();

            if (cardPrefabs == null || cardPrefabs.Count == 0)
            {
                Debug.LogError("Card prefabs not set!");
                return;
            }

            foreach (var cardPrefab in cardPrefabs)
            {
                for (int i = 0; i < CardsPerType; i++)
                {
                    _availableCards.Add(cardPrefab);
                }
            }

            if (_availableCards.Count != TotalCardCount)
            {
                Debug.LogError($"Card deck setup error: Expected {TotalCardCount} cards, got {_availableCards.Count}");
                return;
            }

            ShuffleCards();
        }
        #endregion

        #region Card Management
        public void SetClosedCardsCount(int count)
        {
            closedCardsCount = Mathf.Min(count, cardPositions.Count);
            _closedCardPositions.Clear();
        }

        // public void SetClosedCardsCountBasedOnChance(int chancePercentage)
        // {
        //     int maxClosedCards = Mathf.CeilToInt((chancePercentage / 100f) * cardPositions.Count);
        //     closedCardsCount = Mathf.Min(maxClosedCards, cardPositions.Count);
        //     _closedCardPositions.Clear();
        // }

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
                ShuffleCards();
                OrderCards();
            }
        }

        public void OrderCards()
        {
            if (_availableCards == null || _availableCards.Count == 0)
            {
                Debug.LogWarning("No available cards to order!");
                return;
            }

            SelectClosedPositions();
            SpawnCards();
        }

        private void SelectClosedPositions()
        {
            _closedCardPositions.Clear();
            List<int> availablePositions = new List<int>();

            for (int i = 0; i < _cardStatus.Count; i++)
            {
                if (_cardStatus[i]) availablePositions.Add(i);
            }

            while (_closedCardPositions.Count < closedCardsCount && availablePositions.Count > 0)
            {
                int randomIndex = Random.Range(0, availablePositions.Count);
                _closedCardPositions.Add(availablePositions[randomIndex]);
                availablePositions.RemoveAt(randomIndex);
            }
        }

        private void SpawnCards()
        {
            Sequence spawnSequence = DOTween.Sequence();
            int currentCardIndex = 0;

            for (int i = 0; i < _cardStatus.Count; i++)
            {
                if (!_cardStatus[i] || currentCardIndex >= _availableCards.Count) continue;

                GameObject card = SpawnCard(i, currentCardIndex);
                if (card != null)
                {
                    AnimateCardSpawn(card, i, spawnSequence);
                    currentCardIndex++;
                }
            }

            if (currentCardIndex > 0)
            {
                _availableCards.RemoveRange(0, currentCardIndex);
            }

            CheckDeckStatus();
        }

        private GameObject SpawnCard(int positionIndex, int cardIndex)
        {
            GameObject card = Instantiate(_availableCards[cardIndex], cardPositions[positionIndex], true);
            card.name = $"Card_{positionIndex}";
            card.transform.position = cardPositions[positionIndex].position;
            card.transform.localScale = _cardSpawnScale;

            var cardBehavior = card.GetComponent<CardBehaviours>();
            if (cardBehavior != null)
            {
                cardBehavior.Initialize();
                SetupCardState(card, cardBehavior, _closedCardPositions.Contains(positionIndex));
            }

            _cardStatus[positionIndex] = false;
            return card;
        }

        private void AnimateCardSpawn(GameObject card, int index, Sequence sequence)
        {
            sequence.Insert(index * spawnDelay, card.transform.DOScale(Vector3.one, cardSpawnDuration)
                .SetEase(cardSpawnEase));
            
            if (_closedCardPositions.Contains(index))
            {
                sequence.Insert(index * spawnDelay, card.transform.DORotate(_closedRotation, cardSpawnDuration)
                    .SetEase(cardFlipEase));
            }
        }
        #endregion

        #region Card Interactions
        public void OnCardClicked(GameObject cardObject)
        {
            if (_newGameManager.ForesightCount <= 0) return;
            
            var cardBehavior = cardObject.GetComponent<CardBehaviours>();
            if (cardBehavior == null || cardBehavior.CurrentStatus != CardStatus.Closed) return;

            FlipCard(cardObject, cardBehavior);
        }

        private void FlipCard(GameObject cardObject, CardBehaviours cardBehavior)
        {
            Sequence flipSequence = DOTween.Sequence();

            flipSequence.Append(cardObject.transform.DORotate(new Vector3(0, 180, 0), cardFlipDuration / 2)
                .SetEase(cardFlipEase));

            flipSequence.Append(cardObject.transform.DORotate(_openRotation, cardFlipDuration / 2)
                .SetEase(cardFlipEase));

            flipSequence.Insert(cardFlipDuration * 0.75f, 
                cardObject.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 1, 0.5f));

            flipSequence.OnComplete(() =>
            {
                cardBehavior.CurrentStatus = CardStatus.Opened;
                if (cardBehavior.dragAndDrop != null)
                {
                    cardBehavior.dragAndDrop.canDrag = true;
                    _newGameManager.ChangeForesightCount(-1);
                }
            });
        }
        #endregion

        #region Utility Methods
        private void SetupCardState(GameObject cardObject, CardBehaviours cardBehavior, bool isClosed)
        {
            var dragAndDrop = cardObject.GetComponent<DragAndDrop>();
            if (dragAndDrop != null)
            {
                dragAndDrop.canDrag = !isClosed;
            }

            cardBehavior.CurrentStatus = isClosed ? CardStatus.Closed : CardStatus.Opened;
            if (isClosed)
            {
                cardObject.transform.rotation = Quaternion.Euler(_closedRotation);
                Debug.LogError("Card spawned with state: " + cardBehavior.CurrentStatus);
            }
            else
                cardObject.transform.rotation = Quaternion.Euler(_openRotation);
        }

        private void ShuffleCards()
        {
            int n = _availableCards.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                (_availableCards[k], _availableCards[n]) = (_availableCards[n], _availableCards[k]);
            }
        }

        private void CheckDeckStatus()
        {
            if (_availableCards.Count == 0)
            {
                ReplenishDeckFromDiscardPile();
            }
        }

        private void ReplenishDeckFromDiscardPile()
        {
            if (_newGameManager.discardedCards.Count == 0)
            {
                Debug.LogWarning("No cards available in discard pile!");
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

            ShuffleCards();
            _newGameManager.discardedCards.Clear();
        }
        #endregion
    }
}