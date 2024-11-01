using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Card
{
    public class CardContainer : MonoBehaviour
    {
       [SerializeField] private List<Transform> cardPositions;

       public void OrderCards()
       {
           for (int i = 0; i < cardPositions.Count; i++)
           {
               
           }
       }
       
    }
}
