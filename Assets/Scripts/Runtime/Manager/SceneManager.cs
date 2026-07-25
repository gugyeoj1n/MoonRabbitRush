using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoonRabbitRush
{    
    public class SceneManager : MonoBehaviour
    {
        Scene ActiveScene => EditorSceneManager.GetActiveScene();


        public async UniTask<bool> TransitionTo(int index)
        {
            if (ActiveScene == EditorSceneManager.GetSceneByBuildIndex(index))
                return false;

            try
            {
                
                await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(index);
            }
            catch(Exception ex)
            {
                Debug.LogError($"Scene Transition Error! => {ex}");
            }
            finally
            {

            }

            return true;
        }
    }
}
