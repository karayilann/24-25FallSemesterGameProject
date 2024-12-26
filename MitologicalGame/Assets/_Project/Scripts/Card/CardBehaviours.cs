using System.Collections.Generic;
using _Project.Scripts.BaseAndInterfaces;
using _Project.Scripts.GameManagement;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Card
{
    public class CardBehaviours : MonoBehaviour, IInteractable
    {
        public CardContainer cardContainer;
        public RectTransform cardTransform;
        private List<Transform> _list;
        private int _index;
        private CardType _cardType;
        public CardType CardType => _cardType;
        public CardStatus CurrentStatus { get; set; }
    
        public TextMeshProUGUI cardText;
        public DragAndDrop dragAndDrop;
    
        private bool _isInitialized = false;
        private CardBehaviours _selectedCard;
        private NewGameManager _newGameManager;
        
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
            
            // Drag özelliği artık Initialize'da veya kart açıldığında set edilecek
        }

        public void SetCardType(CardType cardType)
        {
            _cardType = cardType;
        }
    
        public void Initialize()
        {
            if (_isInitialized) return;
            
            cardText.text = _cardType.ToString();
            CurrentStatus = CardStatus.Opened; // Varsayılan durum - CardContainer'da değiştirilebilir
            dragAndDrop.canDrag = CurrentStatus == CardStatus.Opened; // Sadece açık kartlar sürüklenebilir
            
            _isInitialized = true;
        }

        private void OnMouseDown()
        {
            if (CurrentStatus == CardStatus.Closed)
            {
                cardContainer?.OnCardClicked(gameObject);
                return;
            }
        }
    
        public void CheckCard(CardBehaviours card)
        {
            if (!card.dragAndDrop.canDrag || CurrentStatus == CardStatus.Closed || card == this) return;
            if (card.CardType == CardType)
            {
                _selectedCard = card;
                CorrectCard(card);
            }
            else
            {
                _newGameManager.ChangeSuspicionCount(+1);
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
                Debug.Log("2'lü eşleşme yapıldı");
            }
            else
            {
                _newGameManager.ChangeSuspicionCount(+1);
                card.dragAndDrop.isProccessed = false;
                card.dragAndDrop.canDrag = true;
                Debug.LogWarning("Yanlış 2'lü eşleşme yapıldı");
                return;
            }
        }

        private void OnCardsMatch(CardBehaviours card)
        {
            Vector3 targetPos = transform.position;
            Transform targetParent = transform.parent;
        
            card.transform.SetParent(targetParent);
            card.transform.position = targetPos;
        
            Vector3 stackOffset = new Vector3(2f * targetParent.childCount, 2f * targetParent.childCount, 0);
            card.cardTransform.localPosition = stackOffset;

            card.dragAndDrop.isProccessed = true;
            card.dragAndDrop.canDrag = false;
            card.cardContainer._cardStatus[card._index] = true;
        
            _selectedCard = null;
            Debug.Log($"Eşleşme yapıldı - Kart: {card.CardType}, Parent: {targetParent.name}");
        }

        private void MoveToCorrectMatchZone()
        {
            Transform currentParent = transform.parent;
            Vector3 targetPosition = new Vector3(166.710007f, -195.842422f, 90);
        
            // Önce tüm child kartları taşı
            for (int i = currentParent.childCount - 1; i >= 0; i--)
            {
                Transform child = currentParent.GetChild(i);
                child.position = targetPosition + new Vector3(0, 0, -0.1f * i);
                child.SetParent(null);
            
                // Kart bileşenlerini devre dışı bırak
                var cardBehaviour = child.GetComponent<CardBehaviours>();
                if (cardBehaviour != null)
                {
                    cardBehaviour.dragAndDrop.canDrag = false;
                    cardBehaviour.dragAndDrop.isProccessed = true;
                }
            }
            Debug.Log("Kartlar doğru eşleşme alanına taşındı");
        }

        public void DestroyCards()
        {
            //cardContainer.ClearCard(_index);
        }
    
        public void Interact()
        {
            throw new System.NotImplementedException();
        }
    }
}