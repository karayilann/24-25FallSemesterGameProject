using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _Project.Scripts.PropScripts
{
    public class SitToSofa : MonoBehaviour
    {
        public Image image;
        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                image.gameObject.SetActive(false);
                image.fillAmount = 0;
            }
        }

        private void OnTriggerStay(Collider other)
        {
            Debug.Log(other.name);
            if (other.CompareTag("Player"))
            {
                image.gameObject.SetActive(true);
            }
        }
    }
}
