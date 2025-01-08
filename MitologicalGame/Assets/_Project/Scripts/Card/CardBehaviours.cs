using System.Collections.Generic;
using _Project.Scripts.BaseAndInterfaces;
using _Project.Scripts.GameManagement;
using UnityEngine;

namespace _Project.Scripts.Card
{
    public class CardBehaviours : MonoBehaviour
    {
        public CardContainer cardContainer;
        public RectTransform cardTransform;
        private List<Transform> _list;
        private int _index;
        [SerializeField] private CardType _cardType;
        public CardType CardType => _cardType;
        public CardStatus CurrentStatus { get; set; }

        public DragAndDrop dragAndDrop;
        private bool _isInitialized = false;
        private NewGameManager _newGameManager;
        public RectTransform matchZone;

        private void Start()
        {
            cardContainer = FindObjectOfType<CardContainer>();
            if (cardContainer == null)
            {
                Debug.LogError("CardContainer is null");
                return;
            }

            _newGameManager = NewGameManager.Instance;
            _list = cardContainer.cardPositions;
            matchZone = _newGameManager.matchZone;
        }

        public void Initialize()
        {
            if (_isInitialized) return;

            CurrentStatus = CardStatus.Opened;
            dragAndDrop.canDrag = CurrentStatus == CardStatus.Opened;
            _isInitialized = true;
        }

        public void CheckCard(CardBehaviours card)
        {
            if (!card.dragAndDrop.canDrag || CurrentStatus == CardStatus.Closed || card == this) return;
            if (card.CardType == CardType)
            {
                CorrectCard(card);
            }
            else
            {
                _newGameManager.ChangeSuspicionCount(+1);
                card.dragAndDrop.ResetCardPosition();
            }
        }

        private void CorrectCard(CardBehaviours card)
        {
            if (!card.dragAndDrop.canDrag || card.CurrentStatus == CardStatus.Closed) return;

            if (_newGameManager.CheckForCorrectCardType(card.CardType))
            {
                OnCardsMatch(card);
                MoveToCorrectMatchZone();
                _newGameManager.ChangeForesightCount(+1);
                Debug.Log("Matched cards successfully.");
            }
            else
            {
                _newGameManager.ChangeSuspicionCount(+1);
                card.dragAndDrop.ResetCardPosition();
            }
        }

        private void OnCardsMatch(CardBehaviours card)
        {
            Vector3 targetPos = transform.position;
            Transform targetParent = transform.parent;

            card.transform.SetParent(targetParent);
            card.transform.position = targetPos;

            card.dragAndDrop.isProcessed = true;
            card.dragAndDrop.canDrag = false;
        }

        private void MoveToCorrectMatchZone()
        {
            Transform currentParent = transform.parent;

            for (int i = currentParent.childCount - 1; i >= 0; i--)
            {
                Transform child = currentParent.GetChild(i);
                child.SetParent(matchZone);

                RectTransform childRect = child.GetComponent<RectTransform>();
                if (childRect != null)
                {
                    childRect.localScale *= 0.5f;
                    childRect.anchoredPosition = new Vector2(-30f * i, 0);
                }
            }
            Debug.Log("Cards moved to match zone.");
        }
    }
}

