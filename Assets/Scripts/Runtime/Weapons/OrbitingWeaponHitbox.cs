using System.Collections.Generic;
using MoonRabbitRush.Combat;
using MoonRabbitRush.Enemies;
using UnityEngine;

namespace MoonRabbitRush.Weapons
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class OrbitingWeaponHitbox : MonoBehaviour
    {
        private readonly Dictionary<EnemyHealth, float> _nextHitTimes = new();
        [SerializeField] private TimedEffect _impactEffectPrefab;
        private GameObject _source;
        private float _damage;
        private float _hitInterval;
        private Vector3 _initialScale;

        private void Awake()
        {
            _initialScale = transform.localScale;
        }

        public void Configure(
            float damage,
            float hitInterval,
            float sizeMultiplier,
            GameObject source)
        {
            _damage = Mathf.Max(0f, damage);
            _hitInterval = Mathf.Max(0.01f, hitInterval);
            _source = source;
            transform.localScale =
                _initialScale * Mathf.Max(0.01f, sizeMultiplier);
            _nextHitTimes.Clear();
        }

        public void MoveToLocal(Vector2 position)
        {
            transform.localPosition = position;
        }

        public void SetActiveSkillVisual(bool isActive)
        {
            ShockDroneTrailView trailView =
                GetComponent<ShockDroneTrailView>();
            trailView?.SetActiveSkill(isActive);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryDamage(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryDamage(other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();

            if (enemy != null)
            {
                _nextHitTimes.Remove(enemy);
            }
        }

        private void TryDamage(Collider2D other)
        {
            EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();

            if (enemy == null || !enemy.IsAlive)
            {
                return;
            }

            if (_nextHitTimes.TryGetValue(enemy, out float nextHitTime) &&
                Time.time < nextHitTime)
            {
                return;
            }

            Vector2 hitPoint = other.ClosestPoint(transform.position);
            enemy.TakeDamage(new DamageInfo(_damage, hitPoint, _source));
            SpawnImpactEffect(hitPoint);
            _nextHitTimes[enemy] = Time.time + _hitInterval;
        }

        private void SpawnImpactEffect(Vector2 hitPoint)
        {
            if (_impactEffectPrefab == null)
            {
                return;
            }

            PoolType poolType = _impactEffectPrefab.PoolKey;
            if (!PoolingManager.IsRegistered(poolType))
            {
                PoolingManager.RegisterPool(
                    poolType,
                    () => Instantiate(_impactEffectPrefab).gameObject,
                    defaultCapacity: 10,
                    maxSize: 100);
            }

            PoolingManager.GetObject(poolType, out GameObject effectObject);
            effectObject?.transform.SetPositionAndRotation(
                hitPoint,
                Quaternion.identity);
        }
    }
}
