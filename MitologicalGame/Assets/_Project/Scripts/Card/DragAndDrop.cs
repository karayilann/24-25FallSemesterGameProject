using System;
using System.Collections.Generic;
using _Project.Scripts.GameManagement;
using UnityEngine;

namespace _Project.Scripts.Card
{
    public class DragAndDrop : MonoBehaviour
    {
        [Header("Card Settings")] public CardBehaviours cardBehaviours;
        public Collider cardCollider;
        public Vector3 dropOffset;
        public bool canDrag = true;
        public bool isProcessed;
        private bool _isDropped;
        private bool _isInteracted;
        public RectTransform cardTransform;

        [Header("Audio Settings")] public AudioSource audioSource;

        //[0]: Hover Sound, [1]: Drag Sound
        public List<AudioClip> audioClips;

        private Camera _mainCamera;
        private Vector3 _mousePosition;
        private Vector3 _initialPosition;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        private void OnMouseEnter()
        {
            if (_isInteracted) return;
            PlayHoverSound();
        }

        private void OnMouseExit()
        {
            _isInteracted = false;
        }

        private void OnMouseDown()
        {
            _mousePosition = Input.mousePosition - GetMousePosition();
            _initialPosition = transform.position;
            if (transform.CompareTag("DroppedCard"))
            {
                HandleDroppedCardInteractions();
            }
        }

        private void OnMouseDrag()
        {
            if (!canDrag) return;

            Vector3 newPosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition - _mousePosition);
            transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);

            Vector3 localPos = transform.localPosition;
            localPos.z = _isDropped ? 47f : -12f;
            transform.localPosition = localPos;

            PlayHoverSound(1);
            _isInteracted = true;
        }

        private void OnMouseUp()
        {
            if (!canDrag) return;
            CheckForHits();
        }

        private Vector3 GetMousePosition()
        {
            return _mainCamera.WorldToScreenPoint(transform.position);
        }

        private void HandleDroppedCardInteractions()
        {
            Debug.Log("Dropped Card Interactions");
            transform.SetParent(null);
            transform.tag = "Interactable";
        }

        private void PlayHoverSound(int clipIndex = 0)
        {
            if (audioSource != null && !audioSource.isPlaying && !_isInteracted)
            {
                audioSource.PlayOneShot(audioClips[clipIndex]);
            }
        }

        private void CheckForHits()
        {
            var direction = (_mainCamera.transform.position - transform.position).normalized;
            Ray ray = new Ray(transform.position - direction * 100, direction);
            RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);

            if (hits.Length > 0)
            {
                foreach (var hit in hits)
                {
                    if (hit.collider != null)
                    {
                        ProcessHit(hit);
                        return;
                    }
                }
            }

            ResetCardPosition();
        }

        private void ProcessHit(RaycastHit hit)
        {
            Debug.Log($"Hit object: {hit.collider.gameObject.name} with tag: {hit.collider.tag}");

            switch (hit.collider.tag)
            {
                case "DropZone":
                    HandleDropZone(hit.collider.gameObject);
                    break;
                case "DiscardZone":
                    HandleDiscardZone(hit.collider.gameObject);
                    break;
                case "Interactable":
                    HandleInteractable(hit.collider.gameObject);
                    break;
                default:
                    ResetCardPosition();
                    break;
            }
        }

        private void HandleDropZone(GameObject zone)
        {
            Debug.Log("Dropped Card to Drop Zone");
            transform.tag = "DroppedCard";
            transform.SetParent(zone.transform);
            transform.position = zone.transform.position + dropOffset;
            canDrag = true;
            _isDropped = true;
        }

        private void HandleDiscardZone(GameObject zone)
        {
            Debug.Log("Dropped Card to Discard Zone");
            var newGameManager = NewGameManager.Instance;
            _isDropped = false;
            transform.SetParent(zone.transform);
            transform.position = zone.transform.position + dropOffset * 0.5f;
            newGameManager.discardedCards.Add(cardBehaviours);
            cardCollider.enabled = false;
            newGameManager.isDiscarded = true;
            isProcessed = true;
            canDrag = false;
        }

        private void HandleInteractable(GameObject obj)
        {
            Debug.Log("Dropped Card to Interactable Object");
            obj.GetComponent<CardBehaviours>()?.CheckCard(cardBehaviours);
        }

        public void ResetCardPosition()
        {
            transform.position = _initialPosition;
        }
    }
}