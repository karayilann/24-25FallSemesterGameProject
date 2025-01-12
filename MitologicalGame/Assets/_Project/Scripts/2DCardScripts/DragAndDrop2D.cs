using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

namespace _Project.Scripts._2DCardScripts
{
    public class DragAndDrop2D : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Card Settings")]
        public CardBehaviours2D cardBehaviours;
        public Canvas canvas;
        public RectTransform rectTransform;
        public CanvasGroup canvasGroup;
        public Image cardImage;
        public bool canDrag = true;
        public Vector3 dropOffset;
        [HideInInspector] public bool isProcessed;
        
        [Header("Animation Settings")]
        [SerializeField] private float fadeSpeed = 0.3f;
        [SerializeField] private float moveSpeed = 0.5f;
        [SerializeField] private float scaleUpDuration = 0.2f;
        [SerializeField] private float scaleDownDuration = 0.2f;
        [SerializeField] private float hoverScale = 1.1f;
        [SerializeField] private Ease moveEase = Ease.OutBack;
        [SerializeField] private Ease scaleEase = Ease.OutQuad;
        
        [Header("Audio Settings")]
        public AudioSource audioSource;
        public List<AudioClip> audioClips;

        private GameManager2D _gameManager2D;
        private bool _isDragging;
        private bool _isInteracted;
        public bool _isDropped;
        private Vector2 _initialPosition;
        private Vector3 _initialScale;
        private RectTransform _oldParent;
        private float _dragCooldown = 0.1f;
        private float _lastDragTime;

        private void Awake()
        {
            _gameManager2D = GameManager2D.Instance;
            _initialScale = transform.localScale;
            
            if (canvas == null)
                canvas = GetComponentInParent<Canvas>();
            // if (rectTransform == null)
            //     rectTransform = GetComponent<RectTransform>();
            // if (canvasGroup == null)
            //     canvasGroup = GetComponent<CanvasGroup>();
            // if (cardImage == null)
            //     cardImage = GetComponent<Image>();
            // if (cardBehaviours == null)
            //     cardBehaviours = GetComponent<CardBehaviours2D>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!canDrag || _isDragging) return;
            //if (Time.time - _lastDragTime < _dragCooldown) return;

            _isDragging = true;
            //_lastDragTime = Time.time;

            // Kill active tweens
            DOTween.Kill(transform);
            DOTween.Kill(canvasGroup);

            _initialPosition = rectTransform.anchoredPosition;
            _oldParent = (RectTransform)transform.parent;
            
            // Start drag animations
            canvasGroup.DOFade(0.6f, fadeSpeed);
            transform.DOScale(_initialScale * 1.05f, scaleUpDuration).SetEase(scaleEase);
            
            canvasGroup.blocksRaycasts = false;
            PlaySound(1);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!canDrag || !_isDragging) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                canvas.worldCamera,
                out Vector2 localPoint);

            rectTransform.position = canvas.transform.TransformPoint(localPoint);
            
            if (transform.parent != _gameManager2D.dragZone)
            {
                transform.SetParent(_gameManager2D.dragZone);
            }
            
            _isInteracted = true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!canDrag || !_isDragging) return;

            _isDragging = false;
            DOTween.Kill(canvasGroup);
            canvasGroup.DOFade(1f, fadeSpeed);
            canvasGroup.blocksRaycasts = true;

            CheckForUIHits(eventData);
        }

        private void CheckForUIHits(PointerEventData eventData)
        {
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            GameObject hitObject = null;
            foreach (var result in results)
            {
                if (result.gameObject != gameObject)
                {
                    hitObject = result.gameObject;
                    break;
                }
            }

            if (hitObject != null)
            {
                ProcessHit(hitObject);
            }
            else
            {
                ResetCardPosition();
            }
        }

        private void ProcessHit(GameObject hitObject)
        {
            switch (hitObject.tag)
            {
                case "DropZone":
                    HandleDropZone(hitObject);
                    break;
                case "DiscardZone":
                    HandleDiscardZone(hitObject);
                    break;
                case "Interactable":
                    HandleInteractable(hitObject);
                    break;
                default:
                    ResetCardPosition();
                    break;
            }
        }

        private void HandleDropZone(GameObject zone)
        {
            if (zone.transform.childCount >= 1)
            {
                ResetCardPosition();
                return;
            }

            transform.SetParent(zone.transform);
            
            rectTransform.DOMove(zone.transform.position + dropOffset, moveSpeed)
                .SetEase(moveEase)
                .OnComplete(() => {
                    cardImage.raycastTarget = false;
                    canDrag = false;
                    _isDropped = true;
                    _gameManager2D.droppedCards.Add(this.cardBehaviours);
                    _gameManager2D.isDropped = true;
                });

            transform.DOScale(_initialScale, scaleDownDuration).SetEase(scaleEase);
        }

        private void HandleDiscardZone(GameObject zone)
        {
            transform.SetParent(zone.transform);
            
            rectTransform.DOMove(zone.transform.position + dropOffset * 0.5f, moveSpeed)
                .SetEase(moveEase)
                .OnComplete(() => {
                    cardImage.raycastTarget = false;
                    _isDropped = false;
                    _gameManager2D.discardedCards.Add(cardBehaviours);
                    _gameManager2D.isDiscarded = true;
                    isProcessed = true;
                    canDrag = false;
                });

            transform.DOScale(_initialScale * 0.8f, scaleDownDuration).SetEase(scaleEase);
        }

        private void HandleInteractable(GameObject obj)
        {
            obj.GetComponent<CardBehaviours2D>()?.CheckCard(cardBehaviours);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_isInteracted || !canDrag || _isDragging) return;
            
            transform.DOScale(_initialScale * hoverScale, scaleUpDuration).SetEase(scaleEase);
            PlaySound();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!canDrag || _isDragging) return;
            
            transform.DOScale(_initialScale, scaleDownDuration).SetEase(scaleEase);
            _isInteracted = false;
        }

        private void PlaySound(int clipIndex = 0)
        {
            if (audioSource != null && audioClips != null && audioClips.Count > clipIndex && !audioSource.isPlaying && !_isInteracted)
            {
                audioSource.PlayOneShot(audioClips[clipIndex]);
            }
        }

        public void ResetCardPosition()
        {
            cardImage.raycastTarget = false;
            rectTransform.SetParent(_oldParent);
            
            rectTransform.DOAnchorPos(_initialPosition, moveSpeed).SetEase(moveEase).OnComplete( () => {
                transform.DOScale(_initialScale, scaleDownDuration).SetEase(scaleEase).OnComplete( ResetDragState);
            });
        }

        private void ResetDragState()
        {
            canDrag = true;  // Reset drag state
            _isDragging = false;
            _isInteracted = false;
            canvasGroup.blocksRaycasts = true;
            cardImage.raycastTarget = true;
        }

        private void OnDisable()
        {
            DOTween.Kill(transform);
            DOTween.Kill(rectTransform);
            DOTween.Kill(canvasGroup);
            ResetDragState();
        }

        private void OnDestroy()
        {
            DOTween.Kill(transform);
            DOTween.Kill(rectTransform);
            DOTween.Kill(canvasGroup);
        }
    }
}