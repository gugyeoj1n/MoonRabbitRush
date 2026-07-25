using System;
using UnityEngine;

namespace MoonRabbitRush.Combat
{
    public sealed class FallingAreaProjectile : MonoBehaviour
    {
        private Component _targetComponent;
        private IDamageable _target;
        private GameObject _source;
        private Vector2 _startPosition;
        private Vector2 _impactPosition;
        private float _radius;
        private float _damage;
        private float _duration;
        private float _elapsed;
        private bool _isActive;
        private bool _isReleased;

        public event Action<FallingAreaProjectile> Released;

        private void Update()
        {
            if (!_isActive)
            {
                return;
            }

            _elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(_elapsed / _duration);
            float easedProgress = progress * progress;
            transform.position = Vector2.Lerp(
                _startPosition,
                _impactPosition,
                easedProgress);

            if (_elapsed >= _duration)
            {
                Impact();
            }
        }

        public bool Launch(
            Vector2 impactPosition,
            float fallHeight,
            float radius,
            float duration,
            float damage,
            Component target,
            GameObject source)
        {
            if (target is not IDamageable damageable)
            {
                Debug.LogError("Area projectile target must implement IDamageable.", this);
                Release();
                return false;
            }

            _impactPosition = impactPosition;
            _startPosition = impactPosition + Vector2.up * Mathf.Max(0f, fallHeight);
            transform.position = _startPosition;
            _radius = Mathf.Max(0f, radius);
            _duration = Mathf.Max(0.05f, duration);
            _damage = Mathf.Max(0f, damage);
            _targetComponent = target;
            _target = damageable;
            _source = source;
            _elapsed = 0f;
            _isReleased = false;
            _isActive = true;
            return true;
        }

        private void Impact()
        {
            _isActive = false;

            if (_targetComponent != null &&
                _target.IsAlive &&
                Vector2.Distance(_targetComponent.transform.position, _impactPosition) <=
                _radius)
            {
                _target.TakeDamage(
                    new DamageInfo(_damage, _impactPosition, _source));
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
            _isActive = false;

            if (Released != null)
            {
                Released.Invoke(this);
                return;
            }

            Destroy(gameObject);
        }
    }
}
