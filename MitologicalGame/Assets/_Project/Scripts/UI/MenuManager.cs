using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;

namespace _Project.Scripts.UI
{
    public class MenuManager : MonoBehaviour
    {
        public List<Button> buttons;

        public PlayableDirector playableDirector;
        
        [SerializeField] private float slideDistance = 1000f;
        [SerializeField] private float animationDuration = 1f; 
        [SerializeField] private float delayBetweenButtons = 0.1f;
        
        public void StartGame()
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                var button = buttons[i];
                
                float delay = i * delayBetweenButtons;
                
                button.transform.DOLocalMoveX(-slideDistance, animationDuration)
                    .SetDelay(delay)
                    .SetEase(Ease.InOutQuad);
            }
            
            float middlePoint = (buttons.Count * delayBetweenButtons + animationDuration) / 2f;
            DOVirtual.DelayedCall(middlePoint, () => Debug.Log("Deneme"));
        }
    }
}