using System;
using System.Collections;
using System.Collections.Generic;
using _Project.Scripts.BaseAndInterfaces;
using _Project.Scripts.CoreScripts;
using _Project.Scripts.GameManagement;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Card
{
    public class CardBehaviours : MonoBehaviour,IInteractable
    {
        public CardContainer cardContainer;
        private List<Transform> _list;
        private int _index;
        private CardType _cardType;
        public CardType CardType => _cardType;
        
        public TextMeshProUGUI cardText;
        public DragAndDrop dragAndDrop;
        [SerializeField] private Vector3 rotateAnimation;
        
        public List<MeshRenderer> cardMeshRenderers = new List<MeshRenderer>();
        private int _suspicionCount;
        private bool _isInitialized = false;
        
        
        private void Start()
        {
            _suspicionCount = GameManager.Instance.suspicionCount;
            cardContainer = FindObjectOfType<CardContainer>();
            if (cardContainer == null)
            {
                Debug.LogError("CardContainer is null");
                return;
            }
            _list = cardContainer.cardPositions;
            dragAndDrop.canDrag = true;
        }

        public void SetCardType(CardType cardType)
        {
            _cardType = cardType;
        }

        public void Initialize()
        {
            if (_isInitialized) return;
            
            // MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
            // foreach (var mesh in meshRenderers)
            // {
            //     cardMeshRenderers.Add(mesh);
            // }
            cardText.text = _cardType.ToString();
            _isInitialized = true; 
        }
        
        public void CheckCard(CardBehaviours card)
        {
            if (card.CardType == this.CardType)
            {
                CorrectCard(card);
            }
        }

        private void CorrectCard(CardBehaviours card)
        {
            card.transform.SetParent(transform);
            card.transform.position = transform.position;
            if (transform.childCount>4)
            {
                Debug.Log("2 lü eşleşme yapıldı");
                var component = card.GetComponent<DragAndDrop>();
                component.isProccessed = true;
                component.canDrag = true;
            }
        }
  
        public void DestroyCards()
        {
            //cardContainer.ClearCard(_index);
        }

        public void Interact()
        {
            
        }
        
    }
}