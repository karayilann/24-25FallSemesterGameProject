using System.Collections;
using System.Collections.Generic;
using _Project.Scripts._2DCardScripts;
using _Project.Scripts.Card;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
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
        public GameObject suspicionText;
        public GameObject foresightText;
        public GameObject sentenceText;
        public GameObject cardDeck;
        public GameObject discardZone;
        public Button nextRoundButton;
        public AudioSource audioSource;
        public List<GameObject> dropZones;
        public List<AudioClip> audioClips;

        private int currentChapter = 0;
        public bool isPassedChapter;
        public bool isTutorialCompleted = false;
        public bool correctAnswer;
        

        private void Awake()
        {
            // Vignette'i Volume profilinden al
            if (postProcess != null && postProcess.profile.TryGet<Vignette>(out var vingetteEffect))
            {
                _vignette = vingetteEffect;
            }
            else
            {
                Debug.LogError("Vignette effect could not be found in the Volume profile!");
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) && !isPassedChapter)
            {
                AdvanceToNextChapter();
            }
        }
        
        private void ChangeVignetteIntensity(float targetIntensity)
        {
            DOTween.To(() => _vignette.intensity.value, 
                x => _vignette.intensity.value = x, 
                targetIntensity, 
                1f).SetEase(Ease.InOutQuad);
        }
        
        private void ChangeVignetteCenter(Vector2 targetCenter)
        {
            DOTween.To(() => _vignette.center.value, 
                x => _vignette.center.value = x, 
                targetCenter, 
                1f).SetEase(Ease.InOutQuad);
        }
        
        private void ChangeVignetteRoundness(bool isRounded)
        {
            _vignette.rounded.value = isRounded;
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
                case 5:
                    ChapterFive();
                    break;
                case 6:
                    ChapterSix();
                    break;
                case 7:
                    ChapterSeven();
                    break;
                case 8:
                    ChapterEight();
                    break;
                default:
                    EndTutorial();
                    break;
            }
        }

        public void StartTutorial()
        {
            if (isTutorialCompleted)
                return;
            this.gameObject.SetActive(true);
            canvasGroup.enabled = true;
            tutorialText.enabled = true;

            foreach (var dropZone in dropZones)
            {
                dropZone.SetActive(false);
            }

            discardZone.SetActive(false);
            cardDeck.SetActive(false);
            cardLayer.SetActive(false);
            suspicionText.SetActive(false);
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
            
            ChangeVignetteRoundness(true);
            ChangeVignetteIntensity(0.44f);
            ChangeVignetteCenter(new Vector2(0.5f, 0.41f));
            
            //audioSource.Play(audioClips[0]);

            audioSource.clip = audioClips[0];
            audioSource.Play();
            
            tutorialText.text = " Babamın kitaplarında keşfettiğim bu simgeler Albert'ın rüyalarını yorumlamam için bana yardımcı olacak. " +
                                "Her simge 5 temel duygudan birini yansıtıyor." +
                                "\nSevinç, Korku, Hüzün, Acı, Öfke\n" +
                                "Aynı duyguyu yansıtan simgeleri eşleştirmem gerekiyor ama hangi duygular?";
            //StartCoroutine(WaitForSound());
            Debug.Log("Chapter One started.");
        }

        // IEnumerator WaitForSound()
        // {
        //     yield return new WaitUntil((() => !audioSource.isPlaying));
        //     AdvanceToNextChapter();
        // }
        
        private void ChapterTwo()
        {
            isPassedChapter = false;
            sentenceText.SetActive(true);
            
            ChangeVignetteRoundness(false);
            ChangeVignetteIntensity(0.725f);
            ChangeVignetteCenter(new Vector2(0.5f, 0.68f));
            
            audioSource.Stop();
            audioSource.clip = audioClips[1];
            audioSource.Play();
            
            tutorialText.text = "Albert'ın söylediklerini doğru yorumlarsam hangi duygunun vurgulandığını anlarım. Sanırım birden fazla duygu da olabilir.";
            //StartCoroutine(WaitForSound());
            Debug.Log("Chapter Two started.");
        }

        private void ChapterThree()
        {
            audioSource.Stop();
            isPassedChapter = false;
            
            ChangeVignetteRoundness(true);
            ChangeVignetteIntensity(0.44f);
            ChangeVignetteCenter(new Vector2(0.5f, 0.41f));
            
            tutorialText.text = "Yorumladığın duygulara göre eşleşen kartları sürükleyerek üst üste getir. " +
                                "Eşleşen kartları doğru sırayla üst üste getirirsen, Albert'ın rüyasını doğru yorumlamış olursun.";
            
            StartCoroutine(WaitForFirstMatch());
            Debug.Log("Chapter Three started.");
        }
        
        private IEnumerator WaitForFirstMatch()
        {
            canvasGroup.enabled = false;
            yield return new WaitUntil(() => correctAnswer);
            AdvanceToNextChapter();
        }
        
        private void ChapterFour()
        {
            if(!correctAnswer) return;
            isPassedChapter = false;
            canvasGroup.enabled = true;
            
            ChangeVignetteIntensity(0.681f);
            ChangeVignetteRoundness(false);
            ChangeVignetteCenter(new Vector2(0.5f, 0));
            
            tutorialText.text = "Bu rüyada dönen bütün duyguları yakalarsam rüyasındaki ziyaretçiyi öğrenebilirim.";
            Debug.Log("Chapter Four started.");
        }
        
        private void ChapterFive()
        {
            if(!correctAnswer) return;
            isPassedChapter = false;
            
            nextRoundButton.gameObject.SetActive(true);
            nextRoundButton.interactable = true;
            
            ChangeVignetteIntensity(0.81f);
            ChangeVignetteRoundness(true);
            ChangeVignetteCenter(new Vector2(0.89f, 0));
            
            tutorialText.text = "Bu tur yapacaklarını bitirdiğinde sıradaki tura geçebilirsin.";
            Debug.Log("Chapter Five started.");
        }
        
        private void ChapterSix()
        {
            if(!correctAnswer) return;
            isPassedChapter = false;
            
            ChangeVignetteRoundness(true);
            ChangeVignetteIntensity(0.38f);
            ChangeVignetteCenter(new Vector2(0.5f, 0.41f));
            
            tutorialText.text = "Turunu bitirmeden önce alandan iki kartını ayırmak zorundasın.";
            Debug.Log("Chapter Six started.");
        }
        
        private void ChapterSeven()
        {
            if(!correctAnswer) return;
            isPassedChapter = false;
            
            discardZone.SetActive(true);
            
            ChangeVignetteIntensity(1f);
            ChangeVignetteRoundness(true);
            ChangeVignetteCenter(new Vector2(0.91f, 0.22f));
            
            tutorialText.text = "Bunun için sürükleyerek kartını ıskartaya atabilirsin.";
            Debug.Log("Chapter Seven started.");
        }
        
        private void ChapterEight()
        {
            if(!correctAnswer) return;
            isPassedChapter = false;

            foreach (var drop in dropZones)
            {
                drop.SetActive(true);
            }
            
            ChangeVignetteIntensity(0.576f);
            ChangeVignetteRoundness(true);
            ChangeVignetteCenter(new Vector2(0.08f, 0.52f));
            
            tutorialText.text = "Buradaki ceplere yerleştirebilirsen diğer turlarda da kullanabilmeni sağlar.";
            Debug.Log("Chapter Eight started.");
        }
        
        public void EndTutorial()
        {
            isTutorialCompleted = true;
            foreach (var dropZone in dropZones)
            {
                dropZone.SetActive(true);
            }
        
            ChangeVignetteCenter(new Vector2(0.5f, 0.5f));
            ChangeVignetteIntensity(0.3f);
            ChangeVignetteRoundness(false);
            
            discardZone.SetActive(true);
            cardDeck.SetActive(true);
            cardLayer.SetActive(true);
            suspicionText.SetActive(true);
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