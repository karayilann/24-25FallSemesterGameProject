using System;
using UnityEngine;

namespace _Project.Scripts.TutorialScripts
{
    public class GameLogic : MonoBehaviour
    {
        public Camera mainCam;

        private void Update()
        {
            DetectZoom();
        }

        private void DetectZoom()
        {
            if (Input.GetKeyDown(KeyCode.LeftAlt))
            {
                if (Input.GetAxis("Mouse ScrollWheel") > 0)
                {
                    mainCam.orthographicSize -= 1;
                }
                else if (Input.GetAxis("Mouse ScrollWheel") < 0)
                {
                    mainCam.orthographicSize += 1;
                }            
            }    
            
        }
    }
}
