using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;

namespace _Project.Scripts.UI
{
    public class MenuManager : MonoBehaviour
    {
        public Camera mainCamera;
        public GameObject gameCanvas;
        public TextMeshProUGUI startText;
        public Vector3 cameraPosition;
        public Vector3 cameraRotation;
        public PlayableDirector playableDirector;
        public List<Button> buttons;

        [SerializeField] private float slideDistance = 1000f;
        [SerializeField] private float animationDuration = 1f; 
        [SerializeField] private float delayBetweenButtons = 0.1f;
        private bool _canPlay;
        private void Awake()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("Main Camera bulunamadı. Lütfen sahnede bir Camera ekleyin ve tag'ini MainCamera yapın.");
                return;
            }

            cameraPosition = new Vector3(-18.0932007f, 39.7307892f, -33.5708008f);
            cameraRotation = new Vector3(10.6924992f, 150.541504f, 0.00255270908f);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.X) && !_canPlay)
            {
                gameCanvas.SetActive(true);
                _canPlay = true;
                startText.text = "Oyun başladı. Oyunu durdurmak için ESC tuşuna basın.";
            }
            
            if(Input.GetKeyDown(KeyCode.Escape) && _canPlay)
            {
                gameCanvas.SetActive(false);
                _canPlay = false;
            }
        }

        public void StartGame()
        {
            if (buttons == null || buttons.Count == 0)
            {
                Debug.LogWarning("Buton listesi boş. Animasyon çalıştırılamıyor.");
                return;
            }

            for (int i = 0; i < buttons.Count; i++)
            {
                var button = buttons[i];
                if (button == null) continue;

                float delay = i * delayBetweenButtons;

                button.transform.DOLocalMoveX(-slideDistance, animationDuration)
                    .SetDelay(delay)
                    .SetEase(Ease.InOutQuad);
            }

            Sequence sequence = DOTween.Sequence();

            float totalDuration = buttons.Count * delayBetweenButtons + animationDuration;

            sequence.AppendInterval(totalDuration / 2f);
            sequence.Append(mainCamera.transform.DOMove(cameraPosition, 1f).SetEase(Ease.InOutQuad));
            
            sequence.Join(mainCamera.transform.DORotate(cameraRotation, 1f).SetEase(Ease.InOutQuad));
            
            
            sequence.OnComplete(() => startText.text = "Oyuna başlamak için X tuşuna basın.");
        }
        
        
        
    }
}
