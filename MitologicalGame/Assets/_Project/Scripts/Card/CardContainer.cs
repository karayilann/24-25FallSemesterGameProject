using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Project.Scripts.Card
{
    public class CardContainer : MonoBehaviour
    {
        [SerializeField] public List<Transform> cardPositions;
        private Dictionary<int, bool> _cardStatus;
        public GameObject cardPrefab;
        private List<CardType> _requiredCardTypes;

        private void Start()
        {
            _cardStatus = new Dictionary<int, bool>();
            _requiredCardTypes = new List<CardType>();
            
            if (cardPositions.Count == 0) return;
            
            for (int i = 0; i < cardPositions.Count; i++)
            {
                _cardStatus.Add(i, true);
            }

            PrepareRequiredCards();
            OrderCards();
        }

        private void PrepareRequiredCards()
        {
            foreach (CardType type in Enum.GetValues(typeof(CardType)))
            {
                _requiredCardTypes.Add(type);
            }

            int remainingPositions = cardPositions.Count - _requiredCardTypes.Count;
            for (int i = 0; i < remainingPositions; i++)
            {
                CardType randomType = (CardType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(CardType)).Length);
                _requiredCardTypes.Add(randomType);
            }

            _requiredCardTypes = _requiredCardTypes.OrderBy(x => UnityEngine.Random.value).ToList();
        }

        public void OrderCards()
        {
            for (int i = 0; i < _cardStatus.Count; i++)
            {
                if (_cardStatus[i] && i < _requiredCardTypes.Count)
                {
                    GameObject card = Instantiate(cardPrefab, cardPositions[i], true);
                    card.name = "Card " + i;
                    card.transform.position = cardPositions[i].position;
                    
                    var cardBehavior = card.GetComponent<MatchCardBehaviours>();
                    if (cardBehavior != null)
                    {
                        cardBehavior.SetCardType(_requiredCardTypes[i]);
                        cardBehavior.Initialize();
                    }
                    
                    _cardStatus[i] = false;
                }
            }
        }

        public void ClearCard(int index)
        {
            if (cardPositions[index].childCount > 0)
            {
                Transform childCard = cardPositions[index].GetChild(0);
                Destroy(childCard.gameObject);
                _cardStatus[index] = true;

                Debug.Log("Card is removed from: " + cardPositions[index].position);
                Debug.Log("Card status: " + _cardStatus[index]);
            }
            else
            {
                Debug.Log("No card to remove at position: " + cardPositions[index].position);
            }
        }
    }
}