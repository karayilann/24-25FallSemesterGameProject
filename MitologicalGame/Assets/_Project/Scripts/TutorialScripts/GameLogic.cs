using System;
using UnityEngine;

namespace _Project.Scripts.TutorialScripts
{
    public class GameLogic : MonoBehaviour
    {
        public Camera mainCam;
        private bool _isOnGame;
        
        private void Update()
        {
            ChangeCullingMask();
        }

        private void ChangeCullingMask()
        {
            if (Input.GetKeyDown(KeyCode.LeftAlt) && _isOnGame)
            {
                // Everything, Default, TransparentFX, Ignore Raycast, Water, UI, Room
                mainCam.cullingMask = (1 << LayerMask.NameToLayer("Everything")) | 
                                      (1 << LayerMask.NameToLayer("Default")) |
                                      (1 << LayerMask.NameToLayer("TransparentFX")) |
                                      (1 << LayerMask.NameToLayer("Ignore Raycast")) |
                                      (1 << LayerMask.NameToLayer("Water")) |
                                      (1 << LayerMask.NameToLayer("UI")) |
                                      (1 << LayerMask.NameToLayer("Room"));
                
                _isOnGame = false;
                Debug.Log("Culling Mask: " + mainCam.cullingMask);
            }
            else if (Input.GetKeyDown(KeyCode.LeftAlt) && !_isOnGame)
            {
                // Her şeyi al, Room hariç
                int allLayers = ~0; // Tüm layer'ları seç
                int roomLayer = 1 << LayerMask.NameToLayer("Room");
                mainCam.cullingMask = allLayers & ~roomLayer; // Room layer'ı hariç hepsini seç
                
                _isOnGame = true;
                Debug.Log("Culling Mask (Room hariç): " + mainCam.cullingMask);
            }
        }
    }
}