using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MoonRabbitRush.Core;
using MoonRabbitRush.Score;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MoonRabbitRush.UI
{
    public sealed class GameOverUIController : MonoBehaviour
    {
        private const int TitleSceneIndex = 0;

        [Header("State")]
        [SerializeField] private GameStateManager _gameStateManager;

        [Header("View")]
        [SerializeField] private GameObject _gameOverRoot;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _survivalTimeText;
        [SerializeField] private TMP_Text _killCountText;
        [SerializeField] private Button _homeButton;
        [SerializeField] private Button _restartButton;

        private CancellationTokenSource _transitionTokenSource;

        private void Awake()
        {
            _gameOverRoot.SetActive(false);
            _homeButton.onClick.AddListener(HandleHomeClicked);
            _restartButton.onClick.AddListener(HandleRestartClicked);
        }

        private void OnEnable()
        {
            _gameStateManager.StateChanged += HandleStateChanged;
        }

        private void OnDisable()
        {
            _gameStateManager.StateChanged -= HandleStateChanged;
        }

        private void OnDestroy()
        {
            _homeButton.onClick.RemoveListener(HandleHomeClicked);
            _restartButton.onClick.RemoveListener(HandleRestartClicked);
        }

        private void HandleStateChanged(InGameState previous, InGameState current)
        {
            if (current != InGameState.GameOver)
            {
                return;
            }

            RefreshResult();
            _gameOverRoot.SetActive(true);
        }

        private void RefreshResult()
        {
            ScoreManager scoreManager = ScoreManager.Instance;
            if (scoreManager == null)
            {
                Debug.LogWarning("ScoreManager was not found. Game-over results cannot be displayed.", this);
                return;
            }

            int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(scoreManager.SurvivalSeconds));
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            _scoreText.text = scoreManager.CurrentScore.ToString("N0");
            _survivalTimeText.SetText($"{minutes:00}:{seconds:00}");
            _killCountText.text = scoreManager.KillCount.ToString("N0");
        }

        private void HandleHomeClicked()
        {
            BeginTransition(restart: false);
        }

        private void HandleRestartClicked()
        {
            BeginTransition(restart: true);
        }

        private void BeginTransition(bool restart)
        {
            if (_transitionTokenSource != null)
            {
                return;
            }

            _homeButton.interactable = false;
            _restartButton.interactable = false;
            _transitionTokenSource = new CancellationTokenSource();
            RunTransition(restart, _transitionTokenSource.Token).Forget();
        }

        private async UniTaskVoid RunTransition(bool restart, CancellationToken token)
        {
            try
            {
                if (restart)
                {
                    await ManagerRoot.Instance.SceneManager.ReloadCurrent(token);
                }
                else
                {
                    await ManagerRoot.Instance.SceneManager.TransitionTo(TitleSceneIndex, token);
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _transitionTokenSource?.Dispose();
                _transitionTokenSource = null;
            }
        }
    }
}
