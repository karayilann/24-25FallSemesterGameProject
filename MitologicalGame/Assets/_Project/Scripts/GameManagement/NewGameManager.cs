using System.Collections.Generic;
using _Project.Scripts.BaseAndInterfaces;
using _Project.Scripts.Card;
using _Project.Scripts.CoreScripts;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.GameManagement
{
    public class NewGameManager : MonoSingleton<NewGameManager>
    {   
        [Header("UI Elements")]
        public TextMeshProUGUI popUpNotificationText;
        public TextMeshProUGUI foresightCountText;
        public TextMeshProUGUI suspicionCountText;
        
        public GameObject popUpNotification;
        public CardContainer cardContainer;
        
        public bool isDiscarded = false;
        
        [SerializeField] private int foresightCount;
        [SerializeField] private int suspicionCount;
        [SerializeField] private int closedCardCount;

        public List<CardBehaviours> discardedCards;
        public List<CardType> requiredCardTypes;
        [SerializeField] private readonly Dictionary<int, int> _suspicionToClosedCardChance = new Dictionary<int, int>();
        
        public RectTransform matchZone;
        
        public int ForesightCount
        {
            get => foresightCount;
            private set
            {
                foresightCount = value;
                foresightCountText.text = "Öngörü Puanı: " + foresightCount.ToString();
            }
        }

        public int SuspicionCount
        {
            get => suspicionCount; 
            set
            {
                suspicionCount = value;
                suspicionCountText.text = "Şüphe Puanı: " + suspicionCount.ToString();
            }
        }
        

        private void Start()
        {
            _suspicionToClosedCardChance.Add(1, 3); // %3 şans
            _suspicionToClosedCardChance.Add(2, 30); 
            _suspicionToClosedCardChance.Add(3, 50); 
        }

        
        public void NextRound()
        {
            if (isDiscarded && cardContainer != null)
            {
                //int closedCardChance = CalculateClosedCardChance();
                cardContainer.CheckForEmptyPositions();
                isDiscarded = false;
            }
            else
            {
                // popUpNotification.transform.DOLocalMoveY(186.1807f, .5f)
                //     .OnStart(() => popUpNotificationText.text = "Please discard the card first!").OnComplete(() =>
                //     {
                //         popUpNotification.transform.DOLocalMoveY(186.1807f, .5f);
                //     });
            }
        }
        
        public bool CheckForCorrectCardType(CardType cardType)
        {
            if (requiredCardTypes.Contains(cardType))
            {
                Debug.Log("İstenilen kart tipi doğru");
                return true;
            }
            Debug.Log("İstenilen kart tipi yanlış");
            return false;
        }
        
        public void ChangeForesightCount(int amount)
        {
            ForesightCount += amount;
        }
        
        public void ChangeSuspicionCount(int amount)
        {
            SuspicionCount += amount;
        }
        
        public int GetClosedCardChance()
        {
            if (_suspicionToClosedCardChance.TryGetValue(suspicionCount, out int chance))
            {
                return chance;
            }

            return 0;
        }

        
        private int CalculateClosedCardChance()
        {
            int chance = Mathf.Clamp(suspicionCount * 20, 0, 100);
            Debug.Log("Closed card chance calculated: " + chance + "%");
            return chance;
        }
    }
}
