using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Scripts.Card
{
    public class MatchCardBehaviours : MonoBehaviour, IPointerDownHandler
    {
        public CardContainer cardContainer;
        private List<Transform> _list;
        int _index;
        public int Index;

        private void Start()
        {
            // Bu kısım için farklı bir çözüm bulunmalı
            cardContainer = FindObjectOfType<CardContainer>();
            if (cardContainer == null)
            {
                Debug.Log("CardContainer is null");
                return;
            }
            _list = cardContainer.cardPositions;
            Index = _list.FindIndex(i => i.transform.position == transform.position);
            Debug.Log("Gameobject : " + name + " Index: " + Index);
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            var x = _list.FindIndex(i => i.transform.position == transform.position);
            Debug.Log("Gameobject :" + name + "Index: " + x);
            cardContainer.ClearCard(x);
        }
    }
}
