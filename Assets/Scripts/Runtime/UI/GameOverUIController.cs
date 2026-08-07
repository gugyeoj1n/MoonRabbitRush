using MoonRabbitRush.Core;
using UnityEngine;

namespace MoonRabbitRush.UI
{
    public sealed class GameOverUIController : MonoBehaviour
    {
        [SerializeField] private GameStateManager _gameStateManager;

        private void OnEnable()
        {
            _gameStateManager.StateChanged += HandleStateChanged;
        }

        private void OnDisable()
        {
            _gameStateManager.StateChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(InGameState previous, InGameState current)
        {
            if (current != InGameState.GameOver)
            {
                return;
            }

            ManagerRoot.Instance.UIManager.EnablePopup<PopupGameOver>();
        }
    }
}
