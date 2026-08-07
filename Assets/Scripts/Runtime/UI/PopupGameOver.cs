using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MoonRabbitRush.Score;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MoonRabbitRush
{
    public class PopupGameOver : UIPopup
    {
        private const int TitleSceneIndex = 0;

        [Header("Result")]
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _survivalTimeText;
        [SerializeField] private TMP_Text _killCountText;

        [Header("Navigation")]
        [SerializeField] private Button _homeButton;
        [SerializeField] private Button _restartButton;

        private CancellationTokenSource _transitionTokenSource;

        protected override void Awake()
        {
            base.Awake();
            _homeButton.onClick.AddListener(HandleHomeClicked);
            _restartButton.onClick.AddListener(HandleRestartClicked);
        }

        private void OnEnable()
        {
            SetButtonsInteractable(true);
            RefreshResult();
            ManagerRoot.Instance.SoundManager.Play("Audio/SFX/Gameover/Gameover_01");
            ManagerRoot.Instance.SoundManager.Stop("Audio/BGM/Start/Start_02");
        }

        private void OnDestroy()
        {
            _transitionTokenSource?.Cancel();
            _transitionTokenSource?.Dispose();
            _homeButton.onClick.RemoveListener(HandleHomeClicked);
            _restartButton.onClick.RemoveListener(HandleRestartClicked);
        }

        private void RefreshResult()
        {
            ScoreManager scoreManager = ScoreManager.Instance;
            if (scoreManager == null)
            {
                Debug.LogWarning(
                    "ScoreManager was not found. Game-over results cannot be displayed.",
                    this);
                return;
            }

            int totalSeconds = Mathf.Max(
                0,
                Mathf.FloorToInt(scoreManager.SurvivalSeconds));
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

            SetButtonsInteractable(false);
            _transitionTokenSource = new CancellationTokenSource();
            RunTransition(restart, _transitionTokenSource.Token).Forget();
        }

        private async UniTaskVoid RunTransition(
            bool restart,
            CancellationToken cancellationToken)
        {
            try
            {
                if (restart)
                {
                    await ManagerRoot.Instance.SceneManager.ReloadCurrent(
                        cancellationToken);
                }
                else
                {
                    await ManagerRoot.Instance.SceneManager.TransitionTo(
                        TitleSceneIndex,
                        cancellationToken);
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

        private void SetButtonsInteractable(bool interactable)
        {
            _homeButton.interactable = interactable;
            _restartButton.interactable = interactable;
        }
    }
}
