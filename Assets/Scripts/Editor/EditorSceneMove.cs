using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MoonRabbitRush.Editor
{
    public static class EditorSceneMove
    {
        private const string Title = "Assets/Scenes/01_Title.unity";
        private const string Game = "Assets/Scenes/02_Game.unity";

        [MenuItem("Moon Rabbit Rush/Scene/01 Title")]
        private static void OpenTitle()
        {
            OpenScene(Title);
        }

        [MenuItem("Moon Rabbit Rush/Scene/02 Game")]
        private static void OpenGame()
        {
            OpenScene(Game);
        }
        private static void OpenScene(string scenePath)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(scenePath);
            }
        }
    }
}
