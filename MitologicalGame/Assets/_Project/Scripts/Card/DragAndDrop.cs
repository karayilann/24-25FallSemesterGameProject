using System;
using System.Collections.Generic;
using _Project.Scripts.GameManagement;
using UnityEngine;

namespace _Project.Scripts.Card
{
    public class DragAndDrop : MonoBehaviour
    {
        public Vector3 dropOffset;
        
        [Header("Card Settings")]
        public CardBehaviours cardBehaviours;
        public Collider cardCollider;
        public bool canDrag = true;
        public bool isProccessed;
        private bool _isDropped;
        private bool _isInteracted;
        
        [Header("Audio Settings")]
        public AudioSource audioSource;
        
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
            if(_isInteracted)return;
            PlayHoverSound();
        }

        private void OnMouseExit()
        {
            _isInteracted = false;
        }

        private void PlayHoverSound(int clipIndex = 0)
        {
            if (audioSource != null && !audioSource.isPlaying && !_isInteracted)
            {
                audioSource.PlayOneShot(audioClips[clipIndex]);
            }
        }
        
        private Vector3 GetMousePosition()
        {
            return _mainCamera.WorldToScreenPoint(transform.position);
        }

        private void OnMouseDown()
        {
            _mousePosition = Input.mousePosition - GetMousePosition();
            _initialPosition = transform.position;
        }

        private void OnMouseDrag()
        {
            if (!canDrag) return;

            transform.position = _mainCamera.ScreenToWorldPoint(Input.mousePosition - _mousePosition);
            PlayHoverSound(1);
            _isInteracted = true;
        }
        
        private void OnMouseUp()
        {
            if (!canDrag) return;
            CheckForHits();
        
            if (!isProccessed && !_isDropped)
            {
                transform.position = _initialPosition;
            }
        }

        private void CheckForHits()
        {
            var direction = (_mainCamera.transform.position - transform.position).normalized;
            Ray ray = new Ray(transform.position - direction * 100, direction);
            RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);

            if (hits.Length > 0)
            {
                bool hitProcessed = false;
                foreach (var hit in hits)
                {
                    if (hit.collider != null)
                    {
                        ProcessHit(hit);
                        hitProcessed = true;
                        break; // İlk geçerli "hit" işlendiğinde döngüden çık
                    }
                }

                if (!hitProcessed)
                {
                    transform.position = _initialPosition;
                }
            }
            else
            {
                transform.position = _initialPosition;
            }
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
                    transform.position = _initialPosition;
                    break;
            }
        }

        // Buraya atılan kartların yeniden kullanılması için fixle
        private void HandleDropZone(GameObject zone)
        {
            transform.SetParent(zone.transform);
            transform.position = zone.transform.position + dropOffset * zone.transform.childCount;
            canDrag = true;
            _isDropped = true;
        }

        private void HandleDiscardZone(GameObject zone)
        {
            var newGameManager = NewGameManager.Instance;
            
            transform.SetParent(zone.transform);
            transform.position = zone.transform.position + dropOffset * 0.5f;
            newGameManager.discardedCards.Add(cardBehaviours);
            cardCollider.enabled = false;
            newGameManager.isDiscarded = true;
            isProccessed = true;
            canDrag = false;
        }

        private void HandleInteractable(GameObject obj)
        {
            obj.GetComponent<CardBehaviours>()?.CheckCard(cardBehaviours);
        }
    }
}