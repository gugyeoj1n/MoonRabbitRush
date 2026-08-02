using MoonRabbitRush.UI;
using MoonRabbitRush.Waves;
using UnityEngine;

namespace MoonRabbitRush.Enemies.Bosses
{
    public sealed class BossEncounterController : MonoBehaviour
    {
        [SerializeField] private BossAlertController _bossAlert;
        [SerializeField] private EnemySpawner _enemySpawner;
        [SerializeField] private EnemyActor _bossPrefab;
        [SerializeField] private WaveDirector _waveDirector;

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

            _activeBoss = _enemySpawner.Spawn(_bossPrefab);

            if (_activeBoss?.Health != null)
            {
                _activeBoss.Health.Died += HandleBossDied;
            }
        }

        private void HandleBossDied()
        {
            UnsubscribeBossDeath();
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
