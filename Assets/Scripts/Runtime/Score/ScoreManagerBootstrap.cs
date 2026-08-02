using UnityEngine;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

namespace MoonRabbitRush.Score
{
    public static class ScoreManagerBootstrap
    {
        private const int GameSceneBuildIndex = 1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            UnitySceneManager.sceneLoaded -= HandleSceneLoaded;
            UnitySceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode _)
        {
            if (scene.buildIndex != GameSceneBuildIndex ||
                Object.FindAnyObjectByType<ScoreManager>() != null)
            {
                return;
            }

            GameObject scoreManagerObject = new("Score Manager");
            UnitySceneManager.MoveGameObjectToScene(scoreManagerObject, scene);
            scoreManagerObject.AddComponent<ScoreManager>();
        }
    }
}
