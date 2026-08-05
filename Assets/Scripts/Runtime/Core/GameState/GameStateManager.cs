using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MoonRabbitRush.Player;
using MoonRabbitRush.Waves;
using UnityEngine;

namespace MoonRabbitRush.Core
{
    public sealed class GameStateManager : MonoBehaviour
    {
        [SerializeField] private Transform _playerRoot;

        [Header("Death Sequence")]
        [SerializeField, Range(0.05f, 1f)]
        private float _deathSlowMotionScale = 0.3f;
        [SerializeField, Range(0f, 1f)]
        private float _deathSlowMotionPortion = 0.5f;
        [SerializeField, Min(0f)] private float _deathPoseHoldDuration = 0.7f;
        [SerializeField, Range(0.1f, 1f)] private float _deathZoomMultiplier = 0.4f;
        [SerializeField, Min(0f)] private float _deathZoomDuration = 0.25f;

        private PlayerHealth _playerHealth;
        private PlayerSpriteAnimation _playerAnimation;
        private CancellationTokenSource _deathSequenceCts;

        public InGameState CurrentState { get; private set; } =
            InGameState.Playing;
        public bool IsPlaying => CurrentState == InGameState.Playing;

        public event Action<InGameState, InGameState> StateChanged;

        private void Awake()
        {
            ResolvePlayerHealth();
            ApplyTimeScale(CurrentState);
        }

        private void OnEnable()
        {
            ResolvePlayerHealth();

            if (_playerHealth != null)
            {
                _playerHealth.Died += HandlePlayerDied;
            }
        }

        private void OnDisable()
        {
            if (_playerHealth != null)
            {
                _playerHealth.Died -= HandlePlayerDied;
            }
        }

        private void OnDestroy()
        {
            CancelDeathSequence();
            Time.timeScale = 1f;
        }

        public bool TryChangeState(InGameState nextState)
        {
            if (nextState == CurrentState)
            {
                return true;
            }

            if (!CanTransition(CurrentState, nextState))
            {
                Debug.LogWarning(
                    $"Invalid game state transition: {CurrentState} -> {nextState}",
                    this);
                return false;
            }

            InGameState previousState = CurrentState;
            CurrentState = nextState;
            ApplyTimeScale(nextState);
            StateChanged?.Invoke(previousState, nextState);
            return true;
        }

        public bool SetPaused(bool isPaused)
        {
            return TryChangeState(
                isPaused ? InGameState.Paused : InGameState.Playing);
        }

        private static bool CanTransition(
            InGameState currentState,
            InGameState nextState)
        {
            if (nextState == InGameState.GameOver)
            {
                return currentState == InGameState.Dying;
            }

            return currentState switch
            {
                InGameState.Playing =>
                    nextState is InGameState.LevelUp
                        or InGameState.Paused
                        or InGameState.Dying,
                InGameState.LevelUp => nextState == InGameState.Playing,
                InGameState.Paused => nextState == InGameState.Playing,
                InGameState.Dying => nextState == InGameState.GameOver,
                _ => false
            };
        }

        private void ApplyTimeScale(InGameState state)
        {
            Time.timeScale = state switch
            {
                InGameState.Playing => 1f,
                InGameState.Dying => _deathSlowMotionScale,
                _ => 0f
            };
        }

        private void ResolvePlayerHealth()
        {
            if (_playerHealth == null && _playerRoot != null)
            {
                _playerHealth = _playerRoot.GetComponent<PlayerHealth>();
                _playerAnimation =
                    _playerRoot.GetComponentInChildren<PlayerSpriteAnimation>();
            }
        }

        private void HandlePlayerDied()
        {
            if (!TryChangeState(InGameState.Dying))
            {
                return;
            }

            FindAnyObjectByType<WaveDirector>()?.Stop();
            _deathSequenceCts = new CancellationTokenSource();
            PlayDeathSequenceAsync(_deathSequenceCts.Token).Forget();
        }

        private async UniTaskVoid PlayDeathSequenceAsync(
            CancellationToken cancellationToken)
        {
            try
            {
                CameraMaanger cameraManager = ManagerRoot.Instance?.CameraMaanger;
                if (cameraManager != null)
                {
                    cameraManager.ZoomInAsync(
                        _deathZoomMultiplier,
                        _deathZoomDuration,
                        cancellationToken).Forget();
                }

                float animationDuration = _playerAnimation != null
                    ? _playerAnimation.DeathAnimationDuration
                    : 0.875f;
                float slowAnimationDuration =
                    animationDuration * _deathSlowMotionPortion;
                float slowRealDuration =
                    slowAnimationDuration / _deathSlowMotionScale;

                await DelayRealtime(slowRealDuration, cancellationToken);
                Time.timeScale = 1f;

                float normalDuration = animationDuration - slowAnimationDuration;
                await DelayRealtime(normalDuration, cancellationToken);
                await DelayRealtime(_deathPoseHoldDuration, cancellationToken);

                TryChangeState(InGameState.GameOver);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _deathSequenceCts?.Dispose();
                _deathSequenceCts = null;
            }
        }

        private static UniTask DelayRealtime(
            float duration,
            CancellationToken cancellationToken)
        {
            if (duration <= 0f)
            {
                return UniTask.CompletedTask;
            }

            return UniTask.Delay(
                TimeSpan.FromSeconds(duration),
                DelayType.UnscaledDeltaTime,
                PlayerLoopTiming.Update,
                cancellationToken);
        }

        private void CancelDeathSequence()
        {
            if (_deathSequenceCts == null)
            {
                return;
            }

            _deathSequenceCts.Cancel();
            _deathSequenceCts.Dispose();
            _deathSequenceCts = null;
        }
    }
}
