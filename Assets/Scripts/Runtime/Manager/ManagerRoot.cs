using UnityEngine;

namespace MoonRabbitRush
{
    public class ManagerRoot : MonoBehaviour
    {
        public static ManagerRoot Instance { get; private set; }

        public SceneManager SceneManager => Instance._SceneManager;
        private SceneManager _SceneManager;


        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 매니저 할당
            _SceneManager = GetComponentInChildren<SceneManager>();
        }
    }
}
