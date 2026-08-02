using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoonRabbitRush.Score
{
    public static class ScoreManagerBootstrap
    {
        private const int GameSceneBuildIndex = 1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode _)
        {
            if (scene.buildIndex != GameSceneBuildIndex ||
                Object.FindAnyObjectByType<ScoreManager>() != null)
            {
                return;
            }

            GameObject scoreManagerObject = new("Score Manager");
            SceneManager.MoveGameObjectToScene(scoreManagerObject, scene);
            scoreManagerObject.AddComponent<ScoreManager>();
        }
    }
}
