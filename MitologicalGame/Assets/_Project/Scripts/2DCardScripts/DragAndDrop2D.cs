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
        [SerializeField] private float rotationAmount = 30f;
        [SerializeField] private float rotationSpeed = 0.2f;
        [SerializeField] private float smoothFactor;

        [Header("Idle Rotation Settings")]
        [SerializeField] private float idleRotationAmount = 5f;
        [SerializeField] private float idleRotationSpeed = 2f;
        private bool idleRotationEnabled = false;

        [Header("Audio Settings")]
        public AudioSource audioSource;
        public List<AudioClip> audioClips;

        private GameManager2D _gameManager2D;
        private bool _isDragging;
        private bool _isInteracted;
        private bool _isPreviewing;
        public bool _isDropped;
        private Vector2 _initialPosition;
        private Vector3 _initialScale;
        private Quaternion _initialRotation;
        private RectTransform _oldParent;
        private RectTransform _dragZone;

        private void Awake()
        {
            _gameManager2D = GameManager2D.Instance;
            _initialScale = transform.localScale;
            _initialRotation = transform.rotation;
            _dragZone = _gameManager2D.dragZone;

            if (canvas == null)
                canvas = GetComponentInParent<Canvas>();
            _oldParent = (RectTransform)transform.parent;

            StartIdleRotation();
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftAlt) && !_isPreviewing)
            {
                _isPreviewing = true;
            }
            else if (Input.GetKeyUp(KeyCode.LeftAlt))
            {
                ResetCardPreview();
            }
        }

        private void StartIdleRotation()
        {
            if (!idleRotationEnabled)
            {
                idleRotationEnabled = true;
                //PerformIdleRotation();
            }
        }

        private void PerformIdleRotation()
        {
            if (!idleRotationEnabled) return;

            transform.DORotateQuaternion(
                Quaternion.Euler(idleRotationAmount, -idleRotationAmount, idleRotationAmount),
                idleRotationSpeed)
                .SetEase(Ease.InOutSine)
                .OnComplete(() =>
                {
                    transform.DORotateQuaternion(
                        Quaternion.Euler(-idleRotationAmount, idleRotationAmount, -idleRotationAmount),
                        idleRotationSpeed)
                        .SetEase(Ease.InOutSine)
                        .OnComplete(PerformIdleRotation);
                });
        }

        private void StopIdleRotation()
        {
            idleRotationEnabled = false;
            DOTween.Kill(transform);
            transform.DORotateQuaternion(_initialRotation, 0.2f).SetEase(Ease.Linear);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!canDrag || _isDragging || _isPreviewing) return;

            StopIdleRotation();

            _isDragging = true;
            DOTween.Kill(transform);
            DOTween.Kill(canvasGroup);

            _initialPosition = rectTransform.anchoredPosition;

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

            Vector3 targetPosition = canvas.transform.TransformPoint(localPoint);

            rectTransform.position = Vector3.Lerp(rectTransform.position, targetPosition, smoothFactor * Time.deltaTime);

            if (transform.parent != _dragZone)
            {
                transform.SetParent(_dragZone);
            }

            _isInteracted = true;

            float deltaX = eventData.delta.x;
            float rotationZ = Mathf.Clamp(deltaX * rotationAmount, -rotationAmount, rotationAmount);

            transform.DORotateQuaternion(Quaternion.Euler(0, 0, -rotationZ), rotationSpeed).SetEase(Ease.Linear);
        }


        public void OnEndDrag(PointerEventData eventData)
        {
            if (!canDrag || !_isDragging) return;

            _isDragging = false;
            DOTween.Kill(canvasGroup);
            canvasGroup.DOFade(1f, fadeSpeed);
            canvasGroup.blocksRaycasts = true;

            CheckForUIHits(eventData);

            transform.DORotateQuaternion(_initialRotation, rotationSpeed).SetEase(Ease.InOutSine);
            StartIdleRotation();
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
            if (isProcessed) return;

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
                PlaySound(2);
                return;
            }

            canDrag = false;
            cardImage.raycastTarget = false;

            DOTween.Kill(transform);
            DOTween.Kill(rectTransform);

            transform.SetParent(zone.transform);

            Sequence dropSequence = DOTween.Sequence();

            dropSequence.Append(rectTransform.DOMove(zone.transform.position + dropOffset, moveSpeed)
                .SetEase(moveEase));

            dropSequence.Join(transform.DOScale(_initialScale, scaleDownDuration)
                .SetEase(scaleEase));

            dropSequence.OnComplete(() =>
            {
                _isDropped = true;
                if (!_gameManager2D.droppedCards.Contains(cardBehaviours))
                {
                    _gameManager2D.droppedCards.Add(cardBehaviours);
                    _gameManager2D.isDropped = true;
                }
                PlaySound(0);
            });
        }

        private void HandleDiscardZone(GameObject zone)
        {
            canDrag = false;
            cardImage.raycastTarget = false;

            DOTween.Kill(transform);
            DOTween.Kill(rectTransform);

            transform.SetParent(zone.transform);

            Sequence discardSequence = DOTween.Sequence();

            Vector3 targetPosition = zone.transform.position + dropOffset * 0.5f;
    
            discardSequence.Append(rectTransform.DOMove(targetPosition, moveSpeed)
                .SetEase(moveEase));

            discardSequence.Join(transform.DOScale(_initialScale, scaleDownDuration)
                .SetEase(scaleEase));

            discardSequence.Join(canvasGroup.DOFade(0.8f, fadeSpeed));

            discardSequence.OnComplete(() =>
            {
                _isDropped = false;
                isProcessed = true;
                if (!_gameManager2D.discardedCards.Contains(cardBehaviours))
                {
                    _gameManager2D.discardedCards.Add(cardBehaviours);
                    _gameManager2D.isDiscarded = true;
                }
                PlaySound(0);
            });
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

            if (_isPreviewing)
            {
                rectTransform.SetParent(_dragZone);
                transform.DOScale(_initialScale * hoverScale * 1.5f, scaleUpDuration).SetEase(scaleEase);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!canDrag || _isDragging) return;

            ResetCardPreview();
        }

        private void ResetCardPreview()
        {
            transform.SetParent(_oldParent);
            transform.DOScale(_initialScale, scaleDownDuration).SetEase(scaleEase);
            _isPreviewing = false;
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
            canDrag = false;
            _isDragging = true;
            DOTween.Kill(transform);
            DOTween.Kill(rectTransform);
            DOTween.Kill(canvasGroup);

            rectTransform.SetParent(_oldParent);

            Sequence resetSequence = DOTween.Sequence();

            resetSequence.Append(rectTransform.DOAnchorPos(_initialPosition, moveSpeed)
                .SetEase(moveEase));

            resetSequence.Join(transform.DOScale(_initialScale, scaleDownDuration)
                .SetEase(scaleEase));

            resetSequence.Join(canvasGroup.DOFade(1f, fadeSpeed));

            resetSequence.OnComplete(() =>
            {
                ResetDragState();
                StartIdleRotation();
            });
        }

        private void ResetDragState()
        {
            if (!isProcessed)
            {
                canDrag = true;
                _isDragging = false;
                _isInteracted = false;
                canvasGroup.blocksRaycasts = true;
                cardImage.raycastTarget = true;
            }
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
