using System.Collections.Generic;
using _Project.Scripts.BaseAndInterfaces;
using _Project.Scripts.Card;
using UnityEngine;
using DG.Tweening;
using System.Collections;

namespace _Project.Scripts._2DCardScripts
{
    public class CardBehaviours2D : MonoBehaviour
    {
        public CardContainer cardContainer;
        public RectTransform cardTransform;
        private List<Transform> _list;
        private int _index;
        [SerializeField] private CardType _cardType;
        public CardType CardType => _cardType;
        public CardStatus CurrentStatus { get; set; }

        public DragAndDrop2D dragAndDrop;
        private bool _isInitialized = false;
        private GameManager2D _gameManager2D;
        public RectTransform matchZone;

        // DOTween Animation Parameters
        [SerializeField] private float shakeDuration = 0.5f;
        [SerializeField] private float shakeStrength = 30f;
        [SerializeField] private int shakeVibrato = 10;
        [SerializeField] private float shakeRandomness = 90f;
        
        [SerializeField] private float moveToMatchDuration = 0.5f;
        [SerializeField] private float scaleDownDuration = 0.3f;
        [SerializeField] private Ease moveEase = Ease.OutBack;

        private void Start()
        {
            cardContainer = FindObjectOfType<CardContainer>();
            if (cardContainer == null)
            {
                Debug.LogError("CardContainer is null");
                return;
            }

            _gameManager2D = GameManager2D.Instance;
            _list = cardContainer.cardPositions;
            matchZone = _gameManager2D.matchZone;
        }

        public void Initialize()
        {
            if (_isInitialized) return;

            CurrentStatus = CardStatus.Opened;
            dragAndDrop.canDrag = CurrentStatus == CardStatus.Opened;
            _isInitialized = true;
        }

        public void CheckCard(CardBehaviours2D card)
        {
            if (!card.dragAndDrop.canDrag || CurrentStatus == CardStatus.Closed || card == this) return;
            if (card.CardType == CardType)
            {
                Debug.Log("Matched cards successfully.");
                CorrectCard(card);
            }
            else
            {
                StartCoroutine(ShakeAndResetCard(card));
            }
        }

        private IEnumerator ShakeAndResetCard(CardBehaviours2D card)
        {
            // Disable dragging during animation
            card.dragAndDrop.canDrag = false;

            // Shake animation
            Sequence shakeSequence = DOTween.Sequence();
            shakeSequence.Append(card.transform.DOShakePosition(shakeDuration, shakeStrength, shakeVibrato, shakeRandomness, false, true));
            
            yield return shakeSequence.WaitForCompletion();

            _gameManager2D.ChangeSuspicionCount(+1);
            card.dragAndDrop.ResetCardPosition();
            
            // Re-enable dragging after animation
            card.dragAndDrop.canDrag = true;
        }

        private void CorrectCard(CardBehaviours2D card)
        {
            if (!card.dragAndDrop.canDrag || card.CurrentStatus == CardStatus.Closed) return;

            if (_gameManager2D.CheckForCorrectCardType(card.CardType))
            {
                OnCardsMatch(card);
                MoveToCorrectMatchZone();
                _gameManager2D.ChangeForesightCount(+1);
                Debug.Log("Matched cards successfully.");
            }
            else
            {
                StartCoroutine(ShakeAndResetCard(card));
            }
        }

        private void OnCardsMatch(CardBehaviours2D card)
        {
            Vector3 targetPos = transform.position;
            Transform targetParent = transform.parent;

            // Smooth movement to target position
            card.transform.SetParent(targetParent);
            card.transform.DOMove(targetPos, moveToMatchDuration)
                .SetEase(moveEase)
                .OnComplete(() => {
                    card.dragAndDrop.isProcessed = true;
                    card.dragAndDrop.canDrag = false;
                });
        }
        
        private void MoveToCorrectMatchZone()
        {
            Transform currentParent = transform.parent;
            int childCount = currentParent.childCount;
            
            for (int i = childCount - 1; i >= 0; i--)
            {
                Transform child = currentParent.GetChild(i);
                RectTransform childRect = child.GetComponent<RectTransform>();
                
                if (childRect != null)
                {
                    // Store initial values
                    Vector2 targetPosition = new Vector2(30f * matchZone.childCount, 0);
                    Vector3 targetScale = childRect.localScale * 0.5f;
                    
                    // Create sequence for smooth transition
                    Sequence moveSequence = DOTween.Sequence();
                    
                    // Set parent immediately
                    child.SetParent(matchZone);
                    
                    // Add animations to sequence
                    moveSequence.Append(childRect.DOAnchorPos(targetPosition, moveToMatchDuration).SetEase(moveEase));
                    moveSequence.Join(childRect.DOScale(targetScale, scaleDownDuration).SetEase(Ease.InOutQuad));
                }
            }
            Debug.Log("Cards moved to match zone.");
        }

        private void OnDestroy()
        {
            // Kill all tweens associated with this object to prevent memory leaks
            transform.DOKill();
        }
    }
}