using System;
using MoonRabbitRush.Player;
using UnityEngine;

namespace MoonRabbitRush.Core
{
    public sealed class GameStateManager : MonoBehaviour
    {
        [SerializeField] private Transform _playerRoot;

        private PlayerHealth _playerHealth;

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
                return currentState != InGameState.GameOver;
            }

            return currentState switch
            {
                InGameState.Playing =>
                    nextState is InGameState.LevelUp
                        or InGameState.Paused,
                InGameState.LevelUp => nextState == InGameState.Playing,
                InGameState.Paused => nextState == InGameState.Playing,
                _ => false
            };
        }

        private static void ApplyTimeScale(InGameState state)
        {
            Time.timeScale = state == InGameState.Playing ? 1f : 0f;
        }

        private void ResolvePlayerHealth()
        {
            if (_playerHealth == null && _playerRoot != null)
            {
                _playerHealth = _playerRoot.GetComponent<PlayerHealth>();
            }
        }

        private void HandlePlayerDied()
        {
            TryChangeState(InGameState.GameOver);
        }
    }
}
