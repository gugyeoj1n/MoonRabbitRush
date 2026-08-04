using System.Collections.Generic;
using MoonRabbitRush.Combat;
using MoonRabbitRush.Enemies;
using UnityEngine;

namespace MoonRabbitRush.Weapons
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class CrescentBoomerangProjectile : MonoBehaviour
    {
        [SerializeField] private float _spinSpeed = -720f;
        [SerializeField, Min(0.05f)] private float _returnDistance = 0.35f;

        private readonly HashSet<EnemyHealth> _hitEnemies = new();
        private Rigidbody2D _rigidbody;
        private Transform _owner;
        private GameObject _source;
        private Vector3 _initialScale;
        private Vector2 _outboundDirection;
        private Vector2 _origin;
        private float _damage;
        private float _speed;
        private float _maxDistance;
        private float _remainingLifetime;
        private bool _isReturning;
        private bool _isLaunched;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _initialScale = transform.localScale;
        }

        private void FixedUpdate()
        {
            if (!_isLaunched)
            {
                return;
            }

            _remainingLifetime -= Time.fixedDeltaTime;
            if (_remainingLifetime <= 0f || _owner == null)
            {
                Release();
                return;
            }

            Vector2 direction;
            if (_isReturning)
            {
                Vector2 toOwner = (Vector2)_owner.position - _rigidbody.position;
                if (toOwner.sqrMagnitude <= _returnDistance * _returnDistance)
                {
                    Release();
                    return;
                }

                direction = toOwner.normalized;
            }
            else
            {
                direction = _outboundDirection;
                if ((_rigidbody.position - _origin).sqrMagnitude >=
                    _maxDistance * _maxDistance)
                {
                    _isReturning = true;
                    _hitEnemies.Clear();
                    direction =
                        ((Vector2)_owner.position - _rigidbody.position).normalized;
                }
            }

            _rigidbody.MovePosition(
                _rigidbody.position + direction * (_speed * Time.fixedDeltaTime));
            _rigidbody.SetRotation(
                _rigidbody.rotation + _spinSpeed * Time.fixedDeltaTime);
        }

        public void Launch(
            Transform owner,
            Vector2 direction,
            float damage,
            float speed,
            float maxDistance,
            float lifetime,
            float sizeMultiplier,
            GameObject source)
        {
            _owner = owner;
            _source = source;
            _origin = transform.position;
            _outboundDirection = direction.sqrMagnitude > 0f
                ? direction.normalized
                : Vector2.right;
            _damage = Mathf.Max(0f, damage);
            _speed = Mathf.Max(0f, speed);
            _maxDistance = Mathf.Max(0.1f, maxDistance);
            _remainingLifetime = Mathf.Max(0.1f, lifetime);
            transform.localScale =
                _initialScale * Mathf.Max(0.01f, sizeMultiplier);
            _hitEnemies.Clear();
            _isReturning = false;
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
        }

        private void OnDisable()
        {
            _isLaunched = false;
            _owner = null;
            _source = null;
            _hitEnemies.Clear();
            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = Vector2.zero;
                _rigidbody.angularVelocity = 0f;
            }
        }

        private void Release()
        {
            if (!_isLaunched)
            {
                return;
            }

            _isLaunched = false;
            PoolingManager.Release(
                PoolType.ProjectileCrescentBoomerang,
                gameObject);
        }
    }
}
