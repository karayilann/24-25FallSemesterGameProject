using _Project.Scripts.Base___Interfaces;
using UnityEngine;

namespace _Project.Scripts.CoreScripts
{
    public class InteractWithObjects : MonoBehaviour
    {
        public float maxInteractionDistance;
        public Camera mainCamera;
        private void Update()
        {
            InteractRay();
        }

        private void InteractRay()
        {
            var screenPointRay = mainCamera.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;

            bool hasHit = Physics.Raycast(screenPointRay, out hit,maxInteractionDistance);

            if (hasHit)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    var o = hit.transform.gameObject;
                    if (o.TryGetComponent(out IInteractable interactableObject))
                    {
                        interactableObject.Interact();
                    }
                }
            }

        }

    }
}
