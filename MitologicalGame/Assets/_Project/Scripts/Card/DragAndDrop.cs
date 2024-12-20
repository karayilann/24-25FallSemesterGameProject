using System;
using _Project.Scripts.GameManagement;
using UnityEngine;

namespace _Project.Scripts.Card
{
    public class DragAndDrop : MonoBehaviour
    {
        private Vector3 _mousePosition;
        public bool canDrag = true;
        public bool isProccessed;
        private bool _isDropped;
        private Vector3 _initialPosition;
        public Vector3 dropOffset;
        public CardBehaviours cardBehaviours;
        private Camera _mainCamera;
        
        private void Awake()
        {
            _mainCamera = Camera.main;
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
        }
    
        private void OnMouseUp()
        {
            if (!canDrag) return; // Sürüklenemez kartları kontrol etme
            CheckForDropZone();
        
            if (!isProccessed && !_isDropped) // Eğer bir işlem yapılmadıysa başlangıç pozisyonuna dön
            {
                transform.position = _initialPosition;
            }
        }

        void CheckForDropZone()
        {
            var direction = (_mainCamera.transform.position - transform.position).normalized;
            Ray ray = new Ray(transform.position - direction * 100, direction);
            RaycastHit hit;
        
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                ProcessHit(hit);
            }
            else
            {
                transform.position = _initialPosition; // Hiçbir şey bulunamadığında başlangıç pozisyonuna dön
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

        private void HandleDropZone(GameObject zone)
        {
            transform.SetParent(zone.transform);
            transform.position = zone.transform.position + dropOffset * zone.transform.childCount;
            canDrag = true;
            _isDropped = true;
        }

        private void HandleDiscardZone(GameObject zone)
        {
            transform.SetParent(zone.transform);
            transform.position = zone.transform.position + dropOffset * 2;
            NewGameManager.Instance.discardedCards.Add(cardBehaviours.CardType);
            NewGameManager.Instance.isDiscarded = true;
            isProccessed = true;
            canDrag = false;
        }

        private void HandleInteractable(GameObject obj)
        {
            obj.GetComponent<CardBehaviours>()?.CheckCard(cardBehaviours);
        }
    }
}