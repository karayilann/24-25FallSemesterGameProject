using _Project.Scripts.BaseAndInterfaces;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Card
{
    public class DragAndDrop : MonoBehaviour
    {
        private Vector3 _mousePosition;
        public Vector3 offset;
        public bool canDrag = false;
        public bool isProccessed;
        private Vector3 _initialPosition;
        public Vector3 dropOffset;
        public CardBehaviours cardBehaviours;

        private Vector3 GetMousePosition()
        {
            return Camera.main.WorldToScreenPoint(transform.position);
        }

        private void OnMouseDown()
        {
            _mousePosition = Input.mousePosition - GetMousePosition();
            _initialPosition = transform.position;
        }

        private void OnMouseDrag()
        {
            if (!canDrag) return;
            transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition - _mousePosition);
            transform.position += offset;
            //CheckForDropZone();
        }

        private void OnMouseUp()
        {
            CheckForDropZone();
        }

        void CheckForDropZone()
        {
            var direction = (Camera.main.transform.position - transform.position).normalized;
            Ray ray = new Ray(transform.position - direction*100, direction);
            RaycastHit hit;
            
            Debug.DrawLine(transform.position + direction*5, direction + transform.position, Color.red, 1f);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                Debug.Log("Hit object: " + hit.collider.gameObject.name);
                if (hit.collider.CompareTag("DropZone"))
                {
                    var o = hit.collider.gameObject;
                    transform.SetParent(o.transform);
                    transform.position = o.transform.position + dropOffset*o.transform.childCount;
                    isProccessed = true;
                    canDrag = true;
                }
                    
                if (hit.collider.CompareTag("DiscardZone"))
                {
                    var o = hit.collider.gameObject;
                    transform.SetParent(o.transform);
                    transform.position = o.transform.position + dropOffset*o.transform.childCount;
                    isProccessed = true;
                    canDrag = true;
                }
                
                if (hit.collider.CompareTag("Interactable"))
                {
                    var o = hit.collider.gameObject;
                    o.GetComponent<CardBehaviours>().CheckCard(cardBehaviours);
                }          
                
                if(!isProccessed)
                    transform.position = _initialPosition;
                
            }
            else
            {
                Debug.Log("Hiçbir obje bulunamadı.");
            }
        }
    }
}