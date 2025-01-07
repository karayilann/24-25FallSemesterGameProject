using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI; // Değişen import satırı
using UnityEngine.Playables;
using UnityEngine.Serialization;

namespace _Project.Scripts.UI
{
    public class MenuManager : MonoBehaviour
    {
        public List<Button> buttons;

        public PlayableDirector playableDirector;
        
        [SerializeField] private float slideDistance = 1000f; // Ne kadar sola kayacağı
        [SerializeField] private float animationDuration = 1f; // Animasyon süresi
        [SerializeField] private float delayBetweenButtons = 0.1f; // Butonlar arası gecikme
        
        public void StartGame()
        {
            // Her buton için sırayla animasyon başlat
            for (int i = 0; i < buttons.Count; i++)
            {
                var button = buttons[i];
                
                // Her buton için gecikmeyi hesapla
                float delay = i * delayBetweenButtons;
                
                // Butonu sola kaydır
                button.transform.DOLocalMoveX(-slideDistance, animationDuration)
                    .SetDelay(delay)
                    .SetEase(Ease.InOutQuad);
            }
            
            // Animasyonun ortasında PlayableDirector'ı çalıştır
            float middlePoint = (buttons.Count * delayBetweenButtons + animationDuration) / 2f;
            DOVirtual.DelayedCall(middlePoint, () => Debug.Log("Deneme"));
        }
    }
}