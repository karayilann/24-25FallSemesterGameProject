using _Project.Scripts.UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Video;

namespace _Project.Scripts.TutorialScripts
{
    public class OnEndVideo : MonoBehaviour
    {
        public VideoPlayer videoPlayer;
        public MenuManager menuManager;
        private void Start()
        {
            videoPlayer.loopPointReached += EndReached;
            Debug.Log("Video ended.");
        }

        private void EndReached(VideoPlayer source)
        {
            menuManager.ButtonsBack();
            Debug.Log("ButtonsBack method called.");
            this.gameObject.SetActive(false);
        }
    }
}
