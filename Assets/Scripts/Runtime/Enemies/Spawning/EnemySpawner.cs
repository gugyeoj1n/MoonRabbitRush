using MoonRabbitRush.Combat;
using MoonRabbitRush.Progression;
using UnityEngine;

namespace MoonRabbitRush.Enemies
{
    public sealed class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private Transform _playerTarget;
        [SerializeField] private Transform _baseTarget;

        [Header("Spawn Rule")]
        [SerializeField] private Vector2 _center = Vector2.zero;
        [SerializeField, Min(0f)] private float _spawnRadius = 10f;
        [SerializeField, Min(0f)] private float _cameraEdgePadding = 3f;

        private PlayerLootCollector _playerLootCollector;

        public Transform PlayerTarget => _playerTarget;

        private void Awake()
        {
            _playerLootCollector =
                _playerTarget != null
                    ? _playerTarget.GetComponent<PlayerLootCollector>()
                    : null;

            if (!TryResolveReferences())
            {
                enabled = false;
            }
        }

        public EnemyActor Spawn(EnemyActor prefab)
        {
            return Spawn(prefab, ResolveRegularEnemyTarget());
        }

        public EnemyActor Spawn(EnemyActor prefab, Transform target)
        {
            if (prefab == null || target == null || !TryResolveReferences())
            {
                return null;
            }

            float angle = Random.Range(0f, Mathf.PI * 2f);
            Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 spawnPosition = CalculateSpawnPosition(direction);

            return SpawnInternal(prefab, target, spawnPosition, 1f);
        }

        public EnemyActor SpawnAt(
            EnemyActor prefab,
            Transform target,
            Vector2 spawnPosition,
            float healthMultiplier = 1f)
        {
            if (prefab == null || target == null || !TryResolveReferences())
            {
                return null;
            }

            return SpawnInternal(
                prefab,
                target,
                spawnPosition,
                healthMultiplier);
        }

        private EnemyActor SpawnInternal(
            EnemyActor prefab,
            Transform target,
            Vector2 spawnPosition,
            float healthMultiplier)
        {

            PoolType poolType = prefab.PoolKey;
            if (!PoolingManager.IsRegistered(poolType))
            {
                PoolingManager.RegisterPool(
                    poolType,
                    () => Instantiate(prefab).gameObject,
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
            enemy.Initialize(
                target,
                _playerLootCollector,
                healthMultiplier);
            return enemy;
        }

        private Vector2 CalculateSpawnPosition(Vector2 direction)
        {
            Camera worldCamera =
                ManagerRoot.Instance?.CameraMaanger?.MainCamera;
            worldCamera ??= Camera.main;

            if (worldCamera == null || !worldCamera.orthographic)
            {
                return _center + direction * _spawnRadius;
            }

            float halfHeight = worldCamera.orthographicSize;
            float halfWidth = halfHeight * worldCamera.aspect;
            float horizontalDistance = Mathf.Abs(direction.x) > Mathf.Epsilon
                ? halfWidth / Mathf.Abs(direction.x)
                : float.PositiveInfinity;
            float verticalDistance = Mathf.Abs(direction.y) > Mathf.Epsilon
                ? halfHeight / Mathf.Abs(direction.y)
                : float.PositiveInfinity;
            float cameraEdgeDistance =
                Mathf.Min(horizontalDistance, verticalDistance);
            float spawnDistance = Mathf.Max(
                _spawnRadius,
                cameraEdgeDistance + _cameraEdgePadding);

            return (Vector2)worldCamera.transform.position +
                   direction * spawnDistance;
        }

        private Transform ResolveRegularEnemyTarget()
        {
            if (IsAlive(_baseTarget))
            {
                return _baseTarget;
            }

            return IsAlive(_playerTarget) ? _playerTarget : null;
        }

        private static bool IsAlive(Transform target)
        {
            if (target == null)
            {
                return false;
            }

            Component targetComponent =
                target.GetComponent(typeof(IDamageable));
            return targetComponent is IDamageable damageable && damageable.IsAlive;
        }

        private bool TryResolveReferences()
        {
            if (_playerTarget == null)
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
