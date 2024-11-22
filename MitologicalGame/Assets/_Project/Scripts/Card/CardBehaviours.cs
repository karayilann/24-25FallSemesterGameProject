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
        private CardStatus _cardStatus;
        public CardStatus CardStatus => _cardStatus;
        
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
        }

        public void SetCardType(CardType cardType)
        {
            _cardType = cardType;
        }

        private void Update()
        {
            Debug.Log(dragAndDrop.canDrag);
        }

        public void Initialize()
        {
            if (_isInitialized) return;
            
            _cardStatus = CardStatus.Closed;
            MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
            foreach (var mesh in meshRenderers)
            {
                cardMeshRenderers.Add(mesh);
            }
            cardText.text = _cardType.ToString();
            Debug.Log("Initialized Card with Type: " + _cardType + " " + name + " " + cardMeshRenderers.Count );
            
            _isInitialized = true; 
        }
        
        public void DestroyCards()
        {
            cardContainer.ClearCard(_index);
        }

        public void Interact()
        {
            _index = _list.FindIndex(i => i.transform.position == transform.position);
            
            if (_index == -1)
            {
                Debug.LogWarning("Invalid index for GameObject: " + name);
                return;
            }

            if (_cardStatus == CardStatus.Closed)
            {
                _cardStatus = CardStatus.Opened;
                GameManager.Instance.OnCardSelected(this);
                transform.DORotate(rotateAnimation, 1f);
                StartCoroutine(WaitAnimation(1f));
               
            }
            else
            {
                _cardStatus = CardStatus.Closed;
                GameManager.Instance.OnCardDeselected(this);
            }
            
            Debug.Log("CardType: " + _cardType + " Index: " + _index + " Status: " + _cardStatus);
        }
        
        private IEnumerator WaitAnimation(float animationTime)
        {
            yield return new WaitForSeconds(animationTime);
            dragAndDrop.canDrag = true;
            Debug.Log("Can Drag: " + dragAndDrop.canDrag);
        }
        
    }
}