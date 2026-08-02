using System;
using UnityEngine;

namespace MoonRabbitRush.Combat
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class DamageProjectile : MonoBehaviour
    {
        private Rigidbody2D _rigidbody;
        private Component _targetComponent;
        private IDamageable _target;
        private GameObject _source;
        private Vector2 _direction;
        private float _damage;
        private float _speed;
        private float _remainingLifetime;
        private bool _isLaunched;
        private bool _isReleased;

        public event Action<DamageProjectile> Released;

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

            _rigidbody.MovePosition(
                _rigidbody.position + _direction * (_speed * Time.fixedDeltaTime));

            _remainingLifetime -= Time.fixedDeltaTime;

            if (_remainingLifetime <= 0f)
            {
                Release();
            }
        }

        public bool Launch(
            Vector2 direction,
            float damage,
            float speed,
            float lifetime,
            Component target,
            GameObject source)
        {
            if (target is not IDamageable damageable)
            {
                Debug.LogError("Projectile target must implement IDamageable.", this);
                Release();
                return false;
            }

            _direction = direction.sqrMagnitude > 0f
                ? direction.normalized
                : Vector2.right;
            _damage = Mathf.Max(0f, damage);
            _speed = Mathf.Max(0f, speed);
            _remainingLifetime = Mathf.Max(0.05f, lifetime);
            _targetComponent = target;
            _target = damageable;
            _source = source;
            _isLaunched = true;
            _isReleased = false;

            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
            return true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_isLaunched || _targetComponent == null)
            {
                return;
            }

            if (other.isTrigger)
            {
                return;
            }

            Transform targetTransform = _targetComponent.transform;
            bool hitTarget =
                other.transform == targetTransform ||
                other.transform.IsChildOf(targetTransform) ||
                targetTransform.IsChildOf(other.transform);

            if (!hitTarget)
            {
                return;
            }

            if (_target.IsAlive)
            {
                Vector2 hitPoint = other.ClosestPoint(transform.position);
                _target.TakeDamage(new DamageInfo(_damage, hitPoint, _source));
            }

            Release();
        }

        public void Release()
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

            Destroy(gameObject);
        }
    }
}
