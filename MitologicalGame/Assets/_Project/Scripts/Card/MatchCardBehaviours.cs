using System;
using System.Collections.Generic;
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
        private List<Transform> _list;
        int _index;
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
            _cardStatus = (CardStatus)Random.Range(0, Enum.GetValues(typeof(CardStatus)).Length);
            imageComponent.color = SetCardImage();
            //Debug.Log("Initialized Card with Type: " + _cardType);
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
            //cardContainer.ClearCard(_index);
        }
        
        public Color32 SetCardImage()
        {
            if (_cardStatus == CardStatus.Closed)
            {
                return Color.cyan;
            }
            
            if (_cardStatus == CardStatus.Opened)
            {
                switch ((int)_cardType)
                {
                    case 0:
                        return Color.yellow;
                    case 1 :
                        return Color.magenta;
                    case 2:
                        return Color.red;
                    case 3:
                        return Color.green;
                    case 4:
                        return Color.black;
                }
            }

            return Color.white;
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
