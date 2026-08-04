using System;
using System.Collections.Generic;
using MoonRabbitRush.Combat;
using MoonRabbitRush.Enemies;
using UnityEngine;

namespace MoonRabbitRush.Weapons
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class HomingWeaponProjectile : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _turnSpeed = 360f;
        [SerializeField] private TimedEffect _impactEffectPrefab;

        private readonly HashSet<EnemyHealth> _hitEnemies = new();
        private Rigidbody2D _rigidbody;
        private EnemyHealth _target;
        private GameObject _source;
        private Vector2 _direction;
        private float _damage;
        private float _speed;
        private float _targetSearchRange;
        private float _remainingLifetime;
        private int _remainingHits;
        private bool _isLaunched;
        private bool _isReleased;

        public event Action<HomingWeaponProjectile> Released;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            if (!_isLaunched)
            {
                return;
            }

            UpdateTarget();

            if (_target != null)
            {
                Vector2 desiredDirection =
                    ((Vector2)_target.transform.position - _rigidbody.position).normalized;
                float maxRadians =
                    _turnSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime;
                _direction = Vector3.RotateTowards(
                    _direction,
                    desiredDirection,
                    maxRadians,
                    0f).normalized;
            }

            _rigidbody.MovePosition(
                _rigidbody.position + _direction * (_speed * Time.fixedDeltaTime));

            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            _rigidbody.SetRotation(angle);

            _remainingLifetime -= Time.fixedDeltaTime;

            if (_remainingLifetime <= 0f)
            {
                Release();
            }
        }

        public void Launch(
            Vector2 direction,
            EnemyHealth target,
            in WeaponLevelStats stats,
            float damage,
            GameObject source)
        {
            _direction = direction.sqrMagnitude > 0f
                ? direction.normalized
                : Vector2.right;
            _target = target;
            _source = source;
            _damage = damage;
            _speed = stats.ProjectileSpeed;
            _targetSearchRange = stats.Range;
            _remainingLifetime = stats.Duration;
            _remainingHits = stats.PierceCount + 1;
            _hitEnemies.Clear();
            _isReleased = false;
            _isLaunched = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_isLaunched)
            {
                return;
            }

            EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();

            if (enemy == null || !enemy.IsAlive || !_hitEnemies.Add(enemy))
            {
                return;
            }

            Vector2 hitPoint = other.ClosestPoint(transform.position);
            enemy.TakeDamage(new DamageInfo(_damage, hitPoint, _source));
            SpawnImpactEffect(hitPoint);
            _remainingHits--;

            if (_remainingHits <= 0)
            {
                Release();
            }
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

        private void UpdateTarget()
        {
            if (_target != null && _target.IsAlive && !_hitEnemies.Contains(_target))
            {
                return;
            }

            _target = EnemyRegistry.FindClosest(
                _rigidbody.position,
                _targetSearchRange);

            if (_target != null && _hitEnemies.Contains(_target))
            {
                _target = null;
            }
        }

        private void Release()
        {
            if (_isReleased)
            {
                return;
            }

            _isReleased = true;
            _isLaunched = false;

            if (Released != null)
            {
                Released.Invoke(this);
                return;
            }

            PoolingManager.Release(
                PoolType.ProjectileCarrotMissile,
                gameObject);
        }
    }
}
