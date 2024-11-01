using UnityEngine;

namespace _Project.Scripts.Base___Interfaces
{
    public class CharacterBase : MonoBehaviour
    {
        public int Score {
            get;
            set;
        }

        public void IncreaseScore() => Score++;
    }
}
