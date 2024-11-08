using UnityEngine;

namespace _Project.Scripts.CoreScripts
{
    public class MonoSingleton<T>: MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }
                
                if (FindObjectsOfType(typeof(T)) is T[] managers && managers.Length != 0)
                {
                    if (managers.Length == 1)
                    {
                        return _instance = managers[0];
                    }
                }

                var o = new GameObject(typeof(T).Name, typeof(T));
                _instance = o.GetComponent<T>();
                
                return _instance;
            }
      
        }
    }
}
