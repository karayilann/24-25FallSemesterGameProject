using System.Collections.Generic;
using _Project.Scripts.BaseAndInterfaces;
using _Project.Scripts.Card;
using _Project.Scripts.CoreScripts;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.GameManagement
{
    public class NewGameManager : MonoSingleton<NewGameManager>
    {   
        public GameObject popUpNotification;
        public TextMeshProUGUI popUpNotificationText;
        public List<CardType> discardedCards;
        public bool isDiscarded = false;
        public CardContainer cardContainer;
        
        public void NextRound()
        {
            if (isDiscarded)
            {
                cardContainer.CheckForEmptyPositions();
                isDiscarded = false;
            }else
            {
                popUpNotification.transform.DOLocalMoveY(186.1807f, .5f)
                    .OnStart(() => popUpNotificationText.text = "Please discard the card first!").OnComplete(() =>
                    {
                        popUpNotification.transform.DOLocalMoveY(186.1807f, .5f);
                    });
            }
        }
    }
}
