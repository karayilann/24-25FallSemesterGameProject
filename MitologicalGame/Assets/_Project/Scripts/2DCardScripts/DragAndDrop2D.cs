using System;
using System.Collections.Generic;
using _Project.Scripts.GameManagement;
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
        [HideInInspector] public bool isProcessed;
        public Vector3 dropOffset;
        
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
        public List<AudioClip> audioClips; //[0]: Hover Sound, [1]: Drag Sound

        private GameManager2D _gameManager2D;
        [HideInInspector] public bool _isDropped;
        private bool _isInteracted;
        private Vector2 _initialPosition;
        private RectTransform _oldParent;
        private Vector3 _initialScale;

        private void Awake()
        {
            if (canvas == null)
                canvas = GetComponentInParent<Canvas>();
            _gameManager2D = GameManager2D.Instance;
            _initialScale = transform.localScale;
        }

        private void Start()
        {
            if (cardBehaviours == null)
                cardBehaviours = GetComponent<CardBehaviours2D>();
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
            if (cardImage == null)
                cardImage = GetComponent<Image>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!canDrag) return;

            // Kill all tweens on this object
            DOTween.Kill(transform);
            DOTween.Kill(canvasGroup);

            _initialPosition = rectTransform.anchoredPosition;
            _oldParent = (RectTransform)transform.parent;
            
            // Animate fade and scale separately
            canvasGroup.DOFade(0.6f, fadeSpeed);
            transform.DOScale(_initialScale * 1.05f, scaleUpDuration).SetEase(scaleEase);
            
            canvasGroup.blocksRaycasts = false;
            PlayHoverSound(1);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!canDrag) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                canvas.worldCamera,
                out Vector2 localPoint);

            rectTransform.position = canvas.transform.TransformPoint(localPoint);
            transform.SetParent(_gameManager2D.dragZone);
            _isInteracted = true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!canDrag) return;

            DOTween.Kill(canvasGroup);
            canvasGroup.DOFade(1f, fadeSpeed);
            canvasGroup.blocksRaycasts = true;

            CheckForUIHits(eventData);
        }

        private void CheckForUIHits(PointerEventData eventData)
        {
            GameObject hitObject = null;
            
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

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
            Debug.Log($"Hit object: {hitObject.name} with tag: {hitObject.tag}");

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
            if(zone.transform.childCount >= 1)
            {
                Debug.Log("Drop Zone is full");
                ResetCardPosition();
                return;
            }

            transform.SetParent(zone.transform);
            
            // Create and start animations separately
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

            Debug.Log("Dropped Card to Drop Zone");
        }

        private void HandleDiscardZone(GameObject zone)
        {
            transform.SetParent(zone.transform);
            
            // Create and start animations separately
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

            Debug.Log("Dropped Card to Discard Zone");
        }

        private void HandleInteractable(GameObject obj)
        {
            Debug.Log("Dropped Card to Interactable Object");
            obj.GetComponent<CardBehaviours2D>()?.CheckCard(cardBehaviours);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_isInteracted || !canDrag) return;
            
            // Simple scale animation
            transform.DOScale(_initialScale * hoverScale, scaleUpDuration).SetEase(scaleEase);
            PlayHoverSound();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!canDrag) return;
            
            // Reset scale
            transform.DOScale(_initialScale, scaleDownDuration).SetEase(scaleEase);
            _isInteracted = false;
        }

        private void PlayHoverSound(int clipIndex = 0)
        {
            if (audioSource != null && !audioSource.isPlaying && !_isInteracted)
            {
                audioSource.PlayOneShot(audioClips[clipIndex]);
            }
        }

        public void ResetCardPosition()
        {
            rectTransform.SetParent(_oldParent);
            
            // Create and start animations separately
            rectTransform.DOAnchorPos(_initialPosition, moveSpeed).SetEase(moveEase);
            transform.DOScale(_initialScale, scaleDownDuration).SetEase(scaleEase);
        }

        private void OnDestroy()
        {
            // Clean up all tweens
            DOTween.Kill(transform);
            DOTween.Kill(rectTransform);
            DOTween.Kill(canvasGroup);
        }

        private void OnDisable()
        {
            // Clean up tweens when object is disabled
            DOTween.Kill(transform);
            DOTween.Kill(rectTransform);
            DOTween.Kill(canvasGroup);
        }
    }
}