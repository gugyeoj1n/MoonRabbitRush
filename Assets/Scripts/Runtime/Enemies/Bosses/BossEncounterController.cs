using MoonRabbitRush.UI;
using UnityEngine;

namespace MoonRabbitRush.Enemies.Bosses
{
    public sealed class BossEncounterController : MonoBehaviour
    {
        [SerializeField] private BossAlertController _bossAlert;
        [SerializeField] private EnemySpawner _enemySpawner;
        [SerializeField] private EnemyActor _bossPrefab;

        private EnemyActor _activeBoss;

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
        }
    }
}
