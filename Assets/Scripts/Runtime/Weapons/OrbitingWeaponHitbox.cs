using System.Collections.Generic;
using MoonRabbitRush.Combat;
using MoonRabbitRush.Enemies;
using UnityEngine;

namespace MoonRabbitRush.Weapons
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class OrbitingWeaponHitbox : MonoBehaviour
    {
        private readonly Dictionary<EnemyHealth, float> _nextHitTimes = new();
        private Rigidbody2D _rigidbody;
        private GameObject _source;
        private float _damage;
        private float _hitInterval;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        public void Configure(float damage, float hitInterval, GameObject source)
        {
            _damage = Mathf.Max(0f, damage);
            _hitInterval = Mathf.Max(0.01f, hitInterval);
            _source = source;
            _nextHitTimes.Clear();
        }

        public void MoveTo(Vector2 position)
        {
            _rigidbody.MovePosition(position);
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
            _nextHitTimes[enemy] = Time.time + _hitInterval;
        }
    }
}
