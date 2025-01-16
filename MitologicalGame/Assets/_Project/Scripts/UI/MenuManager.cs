using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace _Project.Scripts.UI
{
    public class MenuManager : MonoBehaviour
    {
     
        [Header("UI Elements")]
        public Camera mainCamera;
        public GameObject gameCanvas;
        public TextMeshProUGUI startText;
        public TextMeshProUGUI playSubtitleText;
        public string playSubtitle;
        public Vector3 cameraPosition;
        public Vector3 cameraRotation;
        public List<Button> buttons;

        [Header("Animation Settings")]
        [SerializeField] private float slideDistance = 1000f;
        [SerializeField] private float animationDuration = 1f; 
        [SerializeField] private float delayBetweenButtons = 0.1f;
        [SerializeField] private float cameraAnimationDuration = 1f;
        [SerializeField] private float textDuration = 0.1f;
        [SerializeField] private float textDelete = 0.1f;
        
        [Header("Post Processing")]
        public Volume postProcess;
        public ChromaticAberration chromatic;
        public Vignette vignette;

        [Header("Post-Processing Settings")]
        [SerializeField] private float targetChromaticIntensity = 1f;
        [SerializeField] private float targetVignetteIntensity = 0.3f;
        [SerializeField] private float initialChromaticIntensity = 0f;
        [SerializeField] private float initialVignetteIntensity = 0.1f;
        
        private bool _canPlay;

        private void Awake()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("Main Camera bulunamadı.");
                return;
            }

            postProcess.profile.TryGet(out chromatic);
            postProcess.profile.TryGet(out vignette);
            
            if (chromatic != null) chromatic.intensity.value = initialChromaticIntensity;
            if (vignette != null) vignette.intensity.value = initialVignetteIntensity;
            
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
                Debug.LogWarning("Buton listesi boş");
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

            StartCoroutine(PlaySubtitle());
            
            Sequence sequence = DOTween.Sequence();
            float totalDuration = buttons.Count * delayBetweenButtons + animationDuration;

            sequence.AppendInterval(totalDuration / 2f);
            
            Sequence parallelSequence = DOTween.Sequence();
            
            parallelSequence.Join(mainCamera.transform.DOMove(cameraPosition, cameraAnimationDuration).SetEase(Ease.InOutQuad));
            parallelSequence.Join(mainCamera.transform.DORotate(cameraRotation, cameraAnimationDuration).SetEase(Ease.InOutQuad));
            
            if (chromatic != null)
            {
                parallelSequence.Join(DOTween.To(() => chromatic.intensity.value,
                    x => chromatic.intensity.value = x,
                    targetChromaticIntensity,
                    1f).SetEase(Ease.InOutQuad));
            }
            
            if (vignette != null)
            {
                parallelSequence.Join(DOTween.To(() => vignette.intensity.value,
                    x => vignette.intensity.value = x,
                    targetVignetteIntensity,
                    1f).SetEase(Ease.InOutQuad));
            }
            
            sequence.Append(parallelSequence);
            
            sequence.OnComplete(() =>
            {
                startText.text = "Oyuna başlamak için X tuşuna basın.";
            });
        }

        public void ButtonsBack()
        {
            if (buttons == null || buttons.Count == 0)
            {
                Debug.LogWarning("Buton listesi boş");
                return;
            }

            DOTween.Kill(mainCamera.transform);
    
            for (int i = 0; i < buttons.Count; i++)
            {
                var button = buttons[i];
                if (button == null) continue;

                float delay = i * delayBetweenButtons;

                button.transform.DOLocalMoveX(0f, animationDuration)
                    .SetDelay(delay)
                    .SetEase(Ease.InOutQuad);
            }

            Sequence sequence = DOTween.Sequence();
            float totalDuration = buttons.Count * delayBetweenButtons + animationDuration;

            sequence.AppendInterval(totalDuration / 2f);
    
            Sequence parallelSequence = DOTween.Sequence();
    
            parallelSequence.Join(mainCamera.transform.DOMove(new Vector3(-143.165192f,52.4572678f,-103.645332f), 1f).SetEase(Ease.InOutQuad));
            parallelSequence.Join(mainCamera.transform.DORotate(new Vector3(10.9019632f,72.810791f,8.69465566e-07f), 1f).SetEase(Ease.InOutQuad));
    
            if (chromatic != null)
            {
                parallelSequence.Join(DOTween.To(() => chromatic.intensity.value,
                    x => chromatic.intensity.value = x,
                    initialChromaticIntensity,
                    1f).SetEase(Ease.InOutQuad));
            }
    
            if (vignette != null)
            {
                parallelSequence.Join(DOTween.To(() => vignette.intensity.value,
                    x => vignette.intensity.value = x,
                    initialVignetteIntensity,
                    1f).SetEase(Ease.InOutQuad));
            }
    
            sequence.Append(parallelSequence);
    
            sequence.OnComplete(() =>
            {
                startText.text = "";
            });
        }

        private IEnumerator PlaySubtitle()
        {
            foreach (var letter in playSubtitle)
            {
                playSubtitleText.text += letter;
                yield return new WaitForSeconds(textDuration);
            }
            
            yield return new WaitForSeconds(textDelete);
            playSubtitleText.text = "";
        }
        
        
    }
}