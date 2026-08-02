using MoonRabbitRush.Player;
using UnityEngine;

namespace MoonRabbitRush.Enemies
{
    public sealed class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private Transform _target;

        [Header("Spawn Rule")]
        [SerializeField] private Vector2 _center = Vector2.zero;
        [SerializeField, Min(0f)] private float _spawnRadius = 10f;

        private void Awake()
        {
            if (!TryResolveReferences())
            {
                enabled = false;
            }
        }

        public EnemyActor Spawn(EnemyActor prefab)
        {
            if (prefab == null || !TryResolveReferences())
            {
                return null;
            }

            float angle = Random.Range(0f, Mathf.PI * 2f);
            Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 spawnPosition = _center + direction * _spawnRadius;

            PoolType poolType = prefab.PoolKey;
            if (!PoolingManager.IsRegistered(poolType))
            {
                PoolingManager.RegisterPool(
                    poolType,
                    () => Instantiate(prefab, transform).gameObject,
                    defaultCapacity: 10,
                    maxSize: 100);
            }

            PoolingManager.GetObject(poolType, out GameObject enemyObject);
            if (enemyObject == null ||
                !enemyObject.TryGetComponent(out EnemyActor enemy))
            {
                return null;
            }

            enemy.transform.SetPositionAndRotation(
                spawnPosition,
                Quaternion.identity);
            enemy.Initialize(_target);
            return enemy;
        }

        private bool TryResolveReferences()
        {
            if (_target == null)
            {
                Debug.LogError("Player target was not found.", this);
                return false;
            }

            return true;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_center, _spawnRadius);
        }
    }
}
