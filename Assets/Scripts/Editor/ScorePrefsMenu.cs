using MoonRabbitRush.Score;
using UnityEditor;
using UnityEngine;

namespace MoonRabbitRush.Editor
{
    public static class ScorePrefsMenu
    {
        [MenuItem("Moon Rabbit Rush/Score/Clear Saved Score")]
        private static void ClearSavedScore()
        {
            bool shouldClear = EditorUtility.DisplayDialog(
                "Clear Saved Score",
                "저장된 점수 기록을 모두 초기화하시겠습니까?",
                "초기화",
                "취소");

            if (!shouldClear)
            {
                return;
            }

            ScoreStorage.ClearAll();
            Debug.Log("[Score] Saved score records cleared.");
        }
    }
}
