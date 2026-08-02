using MoonRabbitRush.Core;
using MoonRabbitRush.Enemies;
using MoonRabbitRush.Enemies.Bosses;
using MoonRabbitRush.Player;
using MoonRabbitRush.Waves;
using UnityEngine;

namespace MoonRabbitRush.Score
{
    public sealed class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        private PlayerHealth _playerHealth;
        private WaveDirector _waveDirector;
        private GameStateManager _gameStateManager;
        private bool _isFinalized;

        public int CurrentScore { get; private set; }
        public int BestScore { get; private set; }
        public int CurrentWave { get; private set; }
        public int KillCount { get; private set; }
        public int BossKillCount { get; private set; }
        public float SurvivalSeconds { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            BestScore = ScoreStorage.LoadBestRecord().Score;
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();

            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            if (_isFinalized)
            {
                return;
            }

            if (_gameStateManager == null || _gameStateManager.IsPlaying)
            {
                SurvivalSeconds += Time.deltaTime;
            }
        }

        public static void RegisterEnemyDefeat(EnemyActor enemy)
        {
            Instance?.HandleEnemyDefeat(enemy);
        }

        private void HandleEnemyDefeat(EnemyActor enemy)
        {
            if (enemy == null || _isFinalized)
            {
                return;
            }

            CurrentScore += enemy.ScoreReward;
            KillCount++;

            if (enemy.GetComponent<BossPatternController>() != null)
            {
                BossKillCount++;
            }
        }

        private void HandleWaveStarted(int wave)
        {
            CurrentWave = Mathf.Max(CurrentWave, wave);
        }

        private void HandlePlayerDied()
        {
            if (_isFinalized)
            {
                return;
            }

            _isFinalized = true;
            ScoreRecord currentRecord = new(
                CurrentScore,
                CurrentWave,
                SurvivalSeconds,
                KillCount,
                BossKillCount);
            ScoreRecord bestRecord = ScoreStorage.SaveRun(currentRecord);
            BestScore = bestRecord.Score;

            Debug.Log(
                $"[Score] Final Score={currentRecord.Score}, Wave={currentRecord.Wave}, " +
                $"Time={currentRecord.SurvivalSeconds:0.0}s, Kills={currentRecord.KillCount}, " +
                $"BossKills={currentRecord.BossKillCount}, BestScore={bestRecord.Score}",
                this);
        }

        private void ResolveReferences()
        {
            _playerHealth ??= FindAnyObjectByType<PlayerHealth>();
            _waveDirector ??= FindAnyObjectByType<WaveDirector>();
            _gameStateManager ??= FindAnyObjectByType<GameStateManager>();
        }

        private void SubscribeEvents()
        {
            if (_playerHealth != null)
            {
                _playerHealth.Died -= HandlePlayerDied;
                _playerHealth.Died += HandlePlayerDied;
            }

            if (_waveDirector != null)
            {
                _waveDirector.WaveStarted -= HandleWaveStarted;
                _waveDirector.WaveStarted += HandleWaveStarted;
            }
        }

        private void UnsubscribeEvents()
        {
            if (_playerHealth != null)
            {
                _playerHealth.Died -= HandlePlayerDied;
            }

            if (_waveDirector != null)
            {
                _waveDirector.WaveStarted -= HandleWaveStarted;
            }
        }
    }
}
