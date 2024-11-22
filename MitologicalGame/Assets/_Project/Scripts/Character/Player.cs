using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Character
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private Transform _character;
        bool _isInteracting;

        public Animator animator;
        
        [SerializeField] private float _movementSpeed;
        [SerializeField] private float _steerSpeed;

        public Image interactButton;
        private bool _isStand;
        
        void Update()
        {
            if ( Input.anyKey && !_isStand)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.IsName("SitToStand") && stateInfo.normalizedTime < 1.0f)
                {
                    return;
                }
                animator.Play("SitToStand");
                _isStand = true;
                return;
                
            }

            if (Input.GetKey(KeyCode.W))
            {
                _character.transform.Translate(0, 0, _movementSpeed);
            }
            else if (Input.GetKey(KeyCode.S))
            {
                _character.transform.Translate(0, 0, -1 * _movementSpeed);
            }

            if (Input.GetKey(KeyCode.A))
            {
                _character.transform.Rotate(0, -1 * _steerSpeed, 0);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                _character.transform.Rotate(0, _steerSpeed, 0);
            }

            Interact();
        }

        
        void Interact()
        {
            if ( _isInteracting && Input.GetKey(KeyCode.E) && _isStand)
            {
                interactButton.fillAmount += 0.0005f;
                if (interactButton.fillAmount == 1)
                {
                    transform.position = new Vector3(0,-7.36999989f,7.78999996f);
                    transform.rotation = Quaternion.Euler(0,0,0);
                    animator.Play("StandToSit");
                    _isStand = false;
                }
            }
        }
        
        
        private void OnTriggerStay(Collider other)
        { 
            if (other.CompareTag("Interactable") )
            {
                Debug.Log("Interacted with: " + other.name);
                _isInteracting = true;
            }
        }
    }
}
