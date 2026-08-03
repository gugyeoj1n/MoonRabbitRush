using System;
using UnityEngine;

namespace MoonRabbitRush
{
    public class ManagerRoot : MonoBehaviour
    {
        public static ManagerRoot Instance { get; private set; }

        public SceneManager SceneManager => _SceneManager;
        private SceneManager _SceneManager;
        public UIManager UIManager => _UIManager;
        private UIManager _UIManager;

        public CameraMaanger CameraMaanger => _CameraMaanger;
        private CameraMaanger _CameraMaanger;

        public SoundManager SoundManager => _SoundManager;
        private SoundManager _SoundManager;

        public event Action OnQuit;
        public Action OnMoveScene;

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
            _SoundManager = GetComponentInChildren<SoundManager>();

            if (_SceneManager == null)
                Debug.LogError("SceneManager is not found in children of ManagerRoot.");
            if (_UIManager == null)
                Debug.LogError("UIManager is not found in children of ManagerRoot.");
            if (_CameraMaanger == null)
                Debug.LogError("CameraMaanger is not found in children of ManagerRoot.");
            if (_SoundManager == null)
                Debug.LogError("SoundManager is not found in children of ManagerRoot.");

            OnQuit += PoolingManager.Clear;
            OnMoveScene += PoolingManager.Clear;
        }

        private void OnDestroy()
        {
            OnQuit -= PoolingManager.Clear;
            OnMoveScene -= PoolingManager.Clear;
        }



        private void OnApplicationQuit()
        {
            OnQuit?.Invoke();
        }
    }
}
