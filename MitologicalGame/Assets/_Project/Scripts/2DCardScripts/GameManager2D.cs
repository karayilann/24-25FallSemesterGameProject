using System.Collections.Generic;
using _Project.Scripts.BaseAndInterfaces;
using _Project.Scripts.Card;
using _Project.Scripts.CoreScripts;
using _Project.Scripts.TutorialScripts;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Video;

namespace _Project.Scripts._2DCardScripts
{
    public class GameManager2D : MonoSingleton<GameManager2D>
    {
       [Header("UI Elements")]
        public TextMeshProUGUI popUpNotificationText;
        public TextMeshProUGUI foresightCountText;
        public TextMeshProUGUI suspicionCountText;
        public TextMeshProUGUI sentenceText;
        
        public RectTransform popUpNotification;
        public CardContainer cardContainer;
        
        public bool isDiscarded = false;
        public bool isDropped = false;
        public RectTransform dragZone;

        public TutorialManager tutorialManager;
        public GameObject gameCanvas;
        public GameObject videoPlayer;
        
        public List<string> sentences;
        
        private int foresightCount;
        private int suspicionCount;
        private int closedCardCount;
        private int _foundedCardCount;

        public List<CardBehaviours2D> discardedCards;
        public List<CardBehaviours2D> droppedCards;
        public List<CardType> requiredCardTypes;
        [SerializeField] private readonly Dictionary<int, int> _suspicionToClosedCardChance = new Dictionary<int, int>();
        
        public RectTransform matchZone;
        
        public int ForesightCount
        {
            get => foresightCount;
            private set
            {
                foresightCount = value;
                foresightCountText.text =  "Öngörü Puanı: " + foresightCount.ToString();
                ChangeSentence();
            }
        }

        public int SuspicionCount
        {
            get => suspicionCount; 
            set
            {
                suspicionCount = value;
                suspicionCountText.text = "Şüphe Puanı: " + suspicionCount.ToString();
                if(sentences != null)
                    ChangeSentence();
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
            if (isDiscarded && isDropped && cardContainer != null)
            {
                int closedCardChance = CalculateClosedCardChance();
                cardContainer.CheckForEmptyPositions();
                ActivateCardColliders();
                isDiscarded = false;
                isDropped = false;
                Debug.Log("Next round started.");
            }
            else if(!isDiscarded)
            {
                popUpNotification.DOAnchorPosY(-50f, .5f)
                    .OnStart(() => popUpNotificationText.text = "Please discard the card first!")
                    .OnComplete(() =>
                    {
                        DOVirtual.DelayedCall(1f, () => {
                            popUpNotification.DOAnchorPosY(128f, .5f);
                        });
                    });
            }
            else if(!isDropped)
            {
                popUpNotification.DOAnchorPosY(-50f, .5f)
                    .OnStart(() => popUpNotificationText.text = "Please drop the card first!").OnComplete(() =>
                    {
                        DOVirtual.DelayedCall(1f, () => {
                            popUpNotification.DOAnchorPosY(128f, .5f);
                        });
                    });
            }
        }

        private void ChangeSentence()
        {
            if (sentences.Count>0)
            {
                sentenceText.text = sentences[0];
                sentences.RemoveAt(0);
                Debug.Log("Sentence changed.");
            }else {
                sentenceText.text = "";
                Debug.Log("Sentence list is null");
                return;
            }
        }
        
        public void PlayVideo()
        {
            if (requiredCardTypes.Count != 0 && _foundedCardCount != 2 ) return;
            videoPlayer.SetActive(true);
            gameCanvas.SetActive(false);
            Debug.Log("Video is playing.");
        }
        
        
        public bool CheckForCorrectCardType(CardType cardType)
        {
            if (requiredCardTypes.Contains(cardType))
            {
                tutorialManager.correctAnswer = true;
                requiredCardTypes.Remove(cardType);
                _foundedCardCount += 1;
                Debug.Log("İstenilen kart tipi doğru ve listeden çıkarıldı." + cardType);
                return true;
            }
            Debug.Log("İstenilen kart tipi yanlış");
            return false;
        }
        
        public void ActivateCardColliders()
        {
            foreach (CardBehaviours2D card in droppedCards)
            {
                var cardDrag = card.dragAndDrop;
                cardDrag.cardImage.raycastTarget = true;
                cardDrag.canDrag = true;
                cardDrag._isDropped = false;
            }
            Debug.Log("Card colliders are activated.");
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
