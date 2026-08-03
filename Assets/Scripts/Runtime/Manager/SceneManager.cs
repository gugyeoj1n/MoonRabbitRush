using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoonRabbitRush
{    
    public class SceneManager : MonoBehaviour
    {
        Scene ActiveScene => UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        float duration = 1.5f;
        public async UniTask<bool> TransitionTo(int index, CancellationToken token)
        {
            if (ActiveScene.buildIndex == index)
                return false;

            PopupForeGround foreGround = ManagerRoot.Instance.UIManager.GetPopup<PopupForeGround>();

            try
            {
                await foreGround.FadeOut(duration, token);
                await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(index);
                await foreGround.FadeIn(duration, token);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Scene Transition Error! => {ex}");
            }
            finally
            {
                ManagerRoot.Instance.OnMoveScene?.Invoke();
            }

            return true;
        }
    }
}
