using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.BaseAndInterfaces;
using _Project.Scripts.Card;
using UnityEngine;

namespace _Project.Scripts._2DCardScripts
{
    public class CardContainer2D : MonoBehaviour
    {
        [SerializeField] public Transform cardParent; // Kartların atanacağı parent obje
        public GameObject cardPrefab;
        private List<CardType> _requiredCardTypes;

        private void Start()
        {
            _requiredCardTypes = new List<CardType>();
            PrepareRequiredCards();
            OrderCards();
        }

        private void PrepareRequiredCards()
        {
            foreach (CardType type in Enum.GetValues(typeof(CardType)))
            {
                _requiredCardTypes.Add(type);
            }

            int remainingCards = cardParent.childCount - _requiredCardTypes.Count;
            for (int i = 0; i < remainingCards; i++)
            {
                CardType randomType = (CardType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(CardType)).Length);
                _requiredCardTypes.Add(randomType);
            }

            _requiredCardTypes = _requiredCardTypes.OrderBy(x => UnityEngine.Random.value).ToList();
        }

        public void OrderCards()
        {
            for (int i = 0; i < cardParent.childCount; i++)
            {
                if (i < _requiredCardTypes.Count)
                {
                    GameObject card = Instantiate(cardPrefab, cardParent);
                    card.name = "Card " + i;

                    var cardBehavior = card.GetComponent<CardBehaviours>();
                    if (cardBehavior != null)
                    {
                        //cardBehavior.SetCardType(_requiredCardTypes[i]);
                        cardBehavior.Initialize();
                    }
                }
            }
        }

        public void ClearCard(int index)
        {
            if (index >= 0 && index < cardParent.childCount)
            {
                Transform childCard = cardParent.GetChild(index);
                Destroy(childCard.gameObject);

                Debug.Log("Card is removed from index: " + index);
            }
            else
            {
                Debug.Log("No card to remove at index: " + index);
            }
        }
    }
}
