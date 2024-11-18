using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Card
{
    public class CardContainer : MonoBehaviour
    {
       [SerializeField] public List<Transform> cardPositions;
       private Dictionary<int,bool> _cardStatus;
       public GameObject cardPrefab;

       private void Start()
       {
           //cardPositions = new List<Transform>();
           _cardStatus = new Dictionary<int, bool>();
           if (cardPositions.Count == 0) return;
           for (int i = 0; i < cardPositions.Count; i++)
           { 
               _cardStatus.Add(i, true);
           }
       }

       public void OrderCards()
       {
           for (int i = 0; i < _cardStatus.Count; i++)
           {
               if (_cardStatus[i])
               {
                   GameObject card = Instantiate(cardPrefab, cardPositions[i], true);
                   card.name = "Card " + i;
                   card.transform.position = cardPositions[i].position;
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
