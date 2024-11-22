using System;
using UnityEngine;

namespace _Project.Scripts.CoreScripts
{
    public class DragAndDrop : MonoBehaviour
    {
        private Vector3 _mousePosition;
        public Vector3 offset;
        public bool canDrag;
        private Vector3 GetMousePosition()
        {
            return Camera.main.WorldToScreenPoint(transform.position);
        }
    
        private void OnMouseDown()
        {
            _mousePosition = Input.mousePosition - GetMousePosition();
        }

        private void OnMouseDrag()
        {
            if (!canDrag) return;
            transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition- _mousePosition);
            transform.position += offset;
        }

        private void OnMouseUp()
        {
            canDrag = false;
        }
    }
}
