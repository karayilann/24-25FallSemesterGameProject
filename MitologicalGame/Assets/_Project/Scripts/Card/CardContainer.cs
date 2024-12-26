using System;
using System.Collections;
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

        [SerializeField] private int closedCardsCount = 0;
        private List<int> closedCardPositions = new List<int>();
        private const int TotalCardCount = 25;

        private readonly Vector3 closedRotation = new Vector3(-180, -270, 0); // Kapalı kart Y ekseninde -270 derece
        private readonly Vector3 openRotation = new Vector3(-180, -90, 0);    // Açık kart Y ekseninde -90 derece

        private void Start()
        {
            InitializeContainer();
            PrepareCardDeck();
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

        public void SetClosedCardsCount(int count)
        {
            closedCardsCount = Mathf.Min(count, cardPositions.Count);
            closedCardPositions.Clear();
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
                ShuffleCards(availableCardTypes);
                OrderCards();
            }
        }

        public void OrderCards()
        {
            int currentCardIndex = 0;
            closedCardPositions.Clear();

            // Randomly select positions for closed cards
            List<int> availablePositions = new List<int>();
            for (int i = 0; i < _cardStatus.Count; i++)
            {
                if (_cardStatus[i]) availablePositions.Add(i);
            }

            for (int i = 0; i < closedCardsCount && availablePositions.Count > 0; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, availablePositions.Count);
                closedCardPositions.Add(availablePositions[randomIndex]);
                availablePositions.RemoveAt(randomIndex);
            }

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
                        
                        bool shouldBeClosed = closedCardPositions.Contains(i);
                        SetupCardState(card, cardBehavior, shouldBeClosed);
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

        private void SetupCardState(GameObject cardObject, CardBehaviours cardBehavior, bool isClosed)
        {
            var dragAndDrop = cardObject.GetComponent<DragAndDrop>();
            if (dragAndDrop != null)
            {
                dragAndDrop.canDrag = !isClosed;
            }

            if (isClosed)
            {
                cardObject.transform.rotation = Quaternion.Euler(closedRotation);
                cardBehavior.CurrentStatus = CardStatus.Closed;
            }
            else
            {
                cardObject.transform.rotation = Quaternion.Euler(openRotation);
                cardBehavior.CurrentStatus = CardStatus.Opened;
            }
        }

        public void OnCardClicked(GameObject cardObject)
        {
            var cardBehavior = cardObject.GetComponent<CardBehaviours>();
            var dragAndDrop = cardObject.GetComponent<DragAndDrop>();

            if (cardBehavior != null && cardBehavior.CurrentStatus == CardStatus.Closed)
            {
                StartCoroutine(FlipCard(cardObject, cardBehavior, dragAndDrop));
            }
        }

        private IEnumerator FlipCard(GameObject cardObject, CardBehaviours cardBehavior, DragAndDrop dragAndDrop)
        {
            float duration = 0.5f;
            float elapsed = 0f;
            Quaternion startRotation = Quaternion.Euler(closedRotation);
            Quaternion targetRotation = Quaternion.Euler(openRotation);

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