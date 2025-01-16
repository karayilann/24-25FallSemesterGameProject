using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace _Project.Scripts.TutorialScripts
{
    public class TutorialManager : MonoBehaviour
    {
        [Header("Post Processing")]
        public Volume postProcess;
        private Vignette _vignette;

        [Header("References")] 
        public TextMeshProUGUI tutorialText;
        public TextMeshProUGUI tutorialKeyText;
        public CanvasGroup canvasGroup;
        public GameObject cardLayer;
        public GameObject suspicionImage;
        public GameObject suspicionText;
        public GameObject foresightImage;
        public GameObject foresightText;
        public GameObject sentenceText;
        public GameObject cardDeck;
        public GameObject discardZone;
        public Button nextRoundButton;
        public List<GameObject> dropZones;

        private int currentChapter = 0;
        public bool isPassedChapter;
        public bool isTutorialCompleted;

        private void Awake()
        {
            postProcess.profile.TryGet(out _vignette);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) && !isPassedChapter)
            {
                AdvanceToNextChapter();
            }
        }

        private void AdvanceToNextChapter()
        {
            isPassedChapter = true;
            currentChapter++;
        
            switch (currentChapter)
            {
                case 1:
                    ChapterOne();
                    break;
                case 2:
                    ChapterTwo();
                    break;
                case 3:
                    ChapterThree();
                    break;
                case 4:
                    ChapterFour();
                    break;
                default:
                    EndTutorial();
                    break;
            }
        }

        public void StartTutorial()
        {
            if(!isTutorialCompleted) return;
            this.gameObject.SetActive(true);
            canvasGroup.enabled = true;
            tutorialText.enabled = true;

            // Disable all elements initially
            foreach (var dropZone in dropZones)
            {
                dropZone.SetActive(false);
            }

            discardZone.SetActive(false);
            cardDeck.SetActive(false);
            cardLayer.SetActive(false);
            suspicionImage.SetActive(false);
            suspicionText.SetActive(false);
            foresightImage.SetActive(false);
            foresightText.SetActive(false);
            sentenceText.SetActive(false);
            nextRoundButton.gameObject.SetActive(false);
            nextRoundButton.interactable = false;
            tutorialKeyText.text = "Space tuşuna basarak ilerleyebilirsiniz.";
            Debug.Log("Tutorial started.");
            currentChapter = 0;
            AdvanceToNextChapter();
        }

        public void ChapterOne()
        {
            isPassedChapter = false;
            cardLayer.SetActive(true);
            _vignette.rounded.value = true;
            tutorialText.text = "Bu bölümde kartları eşleştirmeyi öğreneceksiniz. İlk olarak kartları eşleştirmek için kartları sürükleyip bırakmanız gerekmektedir.";
            Debug.Log("Chapter One started.");
        }

        private void ChapterTwo()
        {
            isPassedChapter = false;
            sentenceText.SetActive(true);
            _vignette.rounded.value = false;
            DOTween.To(() => _vignette.center.value, 
                x => _vignette.center.value = x, 
                new Vector2(0.5f, 0.81f), 
                1f).SetEase(Ease.InOutQuad);
            tutorialText.text = "Üst kısımdaki cümle size ilham olacaktır.";
            Debug.Log("Chapter Two started.");
        }

        private void ChapterThree()
        {
            isPassedChapter = false;
            sentenceText.SetActive(false);
            tutorialText.text = "Kartları eşleştirdiniz. Şimdi bir sonraki bölüme geçebilirsiniz.";
            Debug.Log("Chapter Three started.");
        }

        private void ChapterFour()
        {
            isPassedChapter = false;
            Debug.Log("Chapter Four started.");
            // Add chapter four content
        }

        public void EndTutorial()
        {
            isTutorialCompleted = true;
            foreach (var dropZone in dropZones)
            {
                dropZone.SetActive(true);
            }
        
            // Enable all elements
            discardZone.SetActive(true);
            cardDeck.SetActive(true);
            cardLayer.SetActive(true);
            suspicionImage.SetActive(true);
            suspicionText.SetActive(true);
            foresightImage.SetActive(true);
            foresightText.SetActive(true);
            sentenceText.SetActive(true);
            nextRoundButton.enabled = true;
            nextRoundButton.interactable = true;
        
            this.gameObject.SetActive(false);
            tutorialText.enabled = false;
            canvasGroup.enabled = false;
            tutorialKeyText.text = "Oyun başladı. Oyunu durdurmak için ESC tuşuna basın.";
            Debug.Log("Tutorial ended.");
        }
    }
}