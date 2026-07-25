using UnityEngine;

namespace MoonRabbitRush
{
    public class ManagerRoot : MonoBehaviour
    {
        public static ManagerRoot Instance { get; private set; }

        public SceneManager SceneManager => Instance._SceneManager;
        private SceneManager _SceneManager;
        public UIManager UIManager => Instance._UIManager;
        private UIManager _UIManager;

        public CameraMaanger CameraMaanger => Instance._CameraMaanger;
        private CameraMaanger _CameraMaanger;


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
            _UIManager = GetComponentInChildren<UIManager>();
            _CameraMaanger = GetComponentInChildren<CameraMaanger>();
        }
    }
}
