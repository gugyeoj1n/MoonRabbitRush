using System;
using Cysharp.Threading.Tasks;
using MoonRabbitRush.UI;
using MoonRabbitRush.Waves;
using UnityEngine;

namespace MoonRabbitRush.Enemies.Bosses
{
    public sealed class BossEncounterController : MonoBehaviour
    {
        public event Action<EnemyActor> BossSpawned;
        public event Action BossDefeated;

        [SerializeField] private BossAlertController _bossAlert;
        [SerializeField] private EnemySpawner _enemySpawner;
        [SerializeField] private EnemyActor _bossPrefab;
        [SerializeField] private WaveDirector _waveDirector;
        [SerializeField, Min(0f)] private float _bossDeathShakeDuration = 1f;
        [SerializeField, Min(0f)] private float _bossDeathShakeAmplitude = 0.4f;
        [SerializeField, Min(0f)] private float _bossDeathShakeFrequency = 9f;

        private EnemyActor _activeBoss;

        private void Awake()
        {
            _waveDirector ??= FindAnyObjectByType<WaveDirector>();
        }

        private void OnEnable()
        {
            if (_bossAlert != null)
            {
                _bossAlert.AlertCompleted += SpawnBoss;
            }
        }

        private void OnDisable()
        {
            if (_bossAlert != null)
            {
                _bossAlert.AlertCompleted -= SpawnBoss;
            }

            UnsubscribeBossDeath();
        }

        private void SpawnBoss()
        {
            if (_activeBoss != null && _activeBoss.IsActive)
            {
                return;
            }

            if (_enemySpawner == null || _bossPrefab == null)
            {
                Debug.LogError(
                    "Boss encounter references are incomplete.",
                    this);
                return;
            }

            _activeBoss = _enemySpawner.Spawn(
                _bossPrefab,
                _enemySpawner.PlayerTarget);
            BossSpawned?.Invoke(_activeBoss);

            if (_activeBoss?.Health != null)
            {
                _activeBoss.Health.Died += HandleBossDied;
            }
        }

        private void HandleBossDied()
        {
            HandleBossDiedAsync().Forget();
        }

        private async UniTaskVoid HandleBossDiedAsync()
        {
            float deathDelay = _activeBoss != null
                ? Mathf.Max(0f, _activeBoss.DeathDeactivationDelay)
                : 0f;

            UnsubscribeBossDeath();

            BossDefeated?.Invoke();
            ManagerRoot.Instance?.CameraMaanger?.PlayShake(
                _bossDeathShakeDuration,
                _bossDeathShakeAmplitude,
                _bossDeathShakeFrequency);

            await UniTask.Delay(
                TimeSpan.FromSeconds(deathDelay),
                DelayType.DeltaTime,
                PlayerLoopTiming.Update,
                destroyCancellationToken);

            _activeBoss = null;
            _waveDirector?.CompleteBossEncounter();
        }

        private void UnsubscribeBossDeath()
        {
            if (_activeBoss?.Health == null)
            {
                return;
            }

            _activeBoss.Health.Died -= HandleBossDied;
        }
    }
}
