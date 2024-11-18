using System;
using System.Collections;
using System.Collections.Generic;
using _Project.Scripts.Base___Interfaces;
using _Project.Scripts.GameManagement;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _Project.Scripts.Card
{
    public class MatchCardBehaviours : MonoBehaviour,IInteractable
    {
        public CardContainer cardContainer;
        private List<Transform> _list;
        private int _index;
        private CardType _cardType;
        public CardType CardType => _cardType;
        private CardStatus _cardStatus;
        public CardStatus CardStatus => _cardStatus;
        [SerializeField] private Vector3 _rotateAnimation;
        
        public List<MeshRenderer> cardMeshRenderers = new List<MeshRenderer>();
        private int suspicionCount;
        
        
        private void Start()
        {
            Initialize();
            suspicionCount = GameManager.Instance.suspicionCount;
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
            MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
            foreach (var mesh in meshRenderers)
            {
                cardMeshRenderers.Add(mesh);
            }
            Debug.Log("Initialized Card with Type: " + _cardType + " " + name + " " + cardMeshRenderers.Count );
        }
        //
        // public void OnPointerDown(PointerEventData eventData)
        // {
        //     _index = _list.FindIndex(i => i.transform.position == transform.position);
        //     
        //     if (_index == -1)
        //     {
        //         Debug.LogWarning("Invalid index for GameObject: " + name);
        //         return;
        //     }
        //     
        //     Debug.Log("CardType: " + _cardType + " Index: " + _index + " Status: " + _cardStatus);
        //     
        //     if (_cardStatus == CardStatus.Closed)
        //     {
        //         _cardStatus = CardStatus.Opened;
        //         GameManager.Instance.OnCardSelected(_cardType);
        //     }
        //     else
        //     {
        //         _cardStatus = CardStatus.Closed;
        //         GameManager.Instance.OnCardDeselected(_cardType);
        //     }
        //     Debug.Log("CardType: " + _cardType + " Index: " + _index + " Status: " + _cardStatus);
        //     
        //     StartCoroutine(Timer());
        //     // İsteğe bağlı, kartı temizlemek için kullanılabilir
        // }
     
        private IEnumerator Timer()
        {
            //StartCoroutine(GameManager.Instance.PlayDissolveEffect(gameObject));
            yield return new WaitForSeconds(3f);
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
                transform.DORotate(_rotateAnimation, 1f);
            }
            else
            {
                _cardStatus = CardStatus.Closed;
                GameManager.Instance.OnCardDeselected(this);
            }
            
            Debug.Log("CardType: " + _cardType + " Index: " + _index + " Status: " + _cardStatus);
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
