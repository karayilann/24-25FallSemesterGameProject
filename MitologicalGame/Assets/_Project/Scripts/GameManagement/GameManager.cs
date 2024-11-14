using System.Collections.Generic;
using _Project.Scripts.Card;
using _Project.Scripts.CoreScripts;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.GameManagement
{
    public class GameManager : MonoSingleton<GameManager>
    {
        // Inspector'dan ayarlanabilir kart tipleri listesi
        [SerializeField] private List<CardType> requiredCardTypesList;
        private HashSet<CardType> requiredCardTypes;

        public HashSet<CardType> selectedCardTypes = new HashSet<CardType>();

        private void Awake()
        {
            // List'ten HashSet oluşturuluyor
            requiredCardTypes = new HashSet<CardType>(requiredCardTypesList);
        }
        
        public Text winText;

        // private void Start()
        // {
        //     if (winText != null)
        //     {
        //         winText.gameObject.SetActive(false);
        //     }
        // }

        public void OnCardSelected(CardType cardType)
        {
            selectedCardTypes.Add(cardType);
            if (selectedCardTypes.Count == requiredCardTypesList.Count) CheckWinCondition();
        }

        public void OnCardDeselected(CardType cardType)
        {
            selectedCardTypes.Remove(cardType);
        }

        private void CheckWinCondition()
        {
            if (requiredCardTypes.IsSubsetOf(selectedCardTypes))
            {
                Debug.Log("Kazandın!");
            }
            else
            {
                Debug.Log("Henüz kazanmıyorsun.");
            }
        }
    }
}