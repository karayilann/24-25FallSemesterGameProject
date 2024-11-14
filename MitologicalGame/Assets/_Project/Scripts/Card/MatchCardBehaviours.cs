using System;
using System.Collections;
using System.Collections.Generic;
using _Project.Scripts.GameManagement;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _Project.Scripts.Card
{
    public class MatchCardBehaviours : MonoBehaviour, IPointerDownHandler
    {
        public CardContainer cardContainer;
        public Image imageComponent;
        public TextMeshProUGUI cardTypeTMP;
        private List<Transform> _list;
        private int _index;
        private CardType _cardType;
        public CardType CardType => _cardType;
        private CardStatus _cardStatus;
        public CardStatus CardStatus => _cardStatus;
        
        private void Start()
        {
            Initialize();
            cardContainer = FindObjectOfType<CardContainer>();
            if (cardContainer == null)
            {
                Debug.LogError("CardContainer is null");
                return;
            }
            _list = cardContainer.cardPositions;
        }
        
        public void Initialize()
        {
            _cardType = (CardType)Random.Range(0, Enum.GetValues(typeof(CardType)).Length);
            _cardStatus = CardStatus.Closed;
            imageComponent.color = SetCardImage();
            Debug.Log("Initialized Card with Type: " + _cardType + " " + name);
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            _index = _list.FindIndex(i => i.transform.position == transform.position);
            
            if (_index == -1)
            {
                Debug.LogWarning("Invalid index for GameObject: " + name);
                return;
            }
            
            Debug.Log("CardType: " + _cardType + " Index: " + _index + " Status: " + _cardStatus);
            
            if (_cardStatus == CardStatus.Closed)
            {
                _cardStatus = CardStatus.Opened;
                imageComponent.color = SetCardImage();
                cardTypeTMP.text = _cardType.ToString();
                GameManager.Instance.OnCardSelected(_cardType);
            }
            else
            {
                _cardStatus = CardStatus.Closed;
                imageComponent.color = SetCardImage();
                cardTypeTMP.text = "";
                GameManager.Instance.OnCardDeselected(_cardType);
            }

            StartCoroutine(Timer());
            // İsteğe bağlı, kartı temizlemek için kullanılabilir
        }
        
        public Color32 SetCardImage()
        {
            if (_cardStatus == CardStatus.Closed)
            {
                return Color.cyan;
            }
            
            if (_cardStatus == CardStatus.Opened)
            {
                switch (_cardType)
                {
                    case CardType.Fear:
                        return Color.yellow;
                    case CardType.Love:
                        return Color.magenta;
                    case CardType.Hate:
                        return Color.red;
                    case CardType.Joy:
                        return Color.green;
                    case CardType.Sadness:
                        return Color.black;
                    default:
                        return Color.white;
                }
            }

            return Color.white;
        }

        private IEnumerator Timer()
        {
            yield return new WaitForSeconds(3f);
            cardContainer.ClearCard(_index);
        }
    }
    
    public enum CardStatus
    {
        Opened,
        Closed,
    }
    
    public enum CardType
    {
        Fear,
        Love,
        Hate,
        Joy,
        Sadness,
    }
}
