using System.Collections;
using MoonRabbitRush.Player;
using UnityEngine;

namespace MoonRabbitRush.Enemies
{
    public sealed class EnemySpawner : MonoBehaviour
    {
        [Header("Spawn Content")]
        [SerializeField] private EnemySpawnEntry[] _spawnEntries;
        [SerializeField] private Transform _target;

        [Header("Spawn Rule")]
        [SerializeField] private Vector2 _center = Vector2.zero;
        [SerializeField, Min(0f)] private float _spawnRadius = 10f;
        [SerializeField, Min(0.05f)] private float _spawnInterval = 5f;

        private Coroutine _spawnRoutine;

        private void OnEnable()
        {
            if (!TryResolveReferences())
            {
                enabled = false;
                return;
            }

            _spawnRoutine = StartCoroutine(SpawnRoutine());
        }

        private void OnDisable()
        {
            if (_spawnRoutine != null)
            {
                StopCoroutine(_spawnRoutine);
                _spawnRoutine = null;
            }
        }

        public EnemyActor SpawnOne()
        {
            if (!TryResolveReferences())
            {
                return null;
            }

            float angle = Random.Range(0f, Mathf.PI * 2f);
            Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 spawnPosition = _center + direction * _spawnRadius;

            EnemyActor selectedPrefab = SelectEnemyPrefab();

            if (selectedPrefab == null)
            {
                return null;
            }

            EnemyActor enemy = Instantiate(
                selectedPrefab,
                spawnPosition,
                Quaternion.identity,
                transform);
            enemy.Initialize(_target);
            return enemy;
        }

        private IEnumerator SpawnRoutine()
        {
            var wait = new WaitForSeconds(_spawnInterval);

            while (enabled)
            {
                yield return wait;
                SpawnOne();
            }
        }

        private bool TryResolveReferences()
        {
            if (_spawnEntries == null || _spawnEntries.Length == 0)
            {
                Debug.LogError("Enemy spawn entries are not assigned.", this);
                return false;
            }

            bool hasValidEntry = false;

            foreach (EnemySpawnEntry entry in _spawnEntries)
            {
                if (entry != null && entry.IsValid)
                {
                    hasValidEntry = true;
                    break;
                }
            }

            if (!hasValidEntry)
            {
                Debug.LogError("No valid enemy spawn entry exists.", this);
                return false;
            }

            if (_target == null)
            {
                PlayerHealth player = FindAnyObjectByType<PlayerHealth>();
                _target = player != null ? player.transform : null;
            }

            if (_target == null)
            {
                Debug.LogError("Player target was not found.", this);
                return false;
            }

            return true;
        }

        private EnemyActor SelectEnemyPrefab()
        {
            float totalWeight = 0f;

            foreach (EnemySpawnEntry entry in _spawnEntries)
            {
                if (entry != null && entry.IsValid)
                {
                    totalWeight += entry.Weight;
                }
            }

            if (totalWeight <= 0f)
            {
                return null;
            }

            float selection = Random.Range(0f, totalWeight);
            EnemyActor fallback = null;

            foreach (EnemySpawnEntry entry in _spawnEntries)
            {
                if (entry == null || !entry.IsValid)
                {
                    continue;
                }

                fallback = entry.Prefab;
                selection -= entry.Weight;

                if (selection <= 0f)
                {
                    return entry.Prefab;
                }
            }

            return fallback;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_center, _spawnRadius);
        }
    }
}
