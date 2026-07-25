using MoonRabbitRush.Combat;
using UnityEngine;

namespace MoonRabbitRush.Enemies
{
    public sealed class TargetedProjectileAttack : EnemyBehaviour
    {
        [SerializeField] private DamageProjectile _projectilePrefab;
        [SerializeField] private Transform _muzzle;
        [SerializeField, Min(0f)] private float _attackRange = 7f;
        [SerializeField, Min(0f)] private float _projectileSpeed = 5f;
        [SerializeField, Min(0.05f)] private float _projectileLifetime = 4f;

        private Component _damageTarget;
        private float _nextAttackTime;

        public override void Initialize(Transform target, EnemyStatsData stats)
        {
            base.Initialize(target, stats);
            _damageTarget = target.GetComponent(typeof(IDamageable)) as Component;
            _nextAttackTime = Time.time + stats.AttackInterval;

            if (_damageTarget == null)
            {
                Debug.LogError("Attack target must implement IDamageable.", this);
            }
        }

        private void Update()
        {
            if (Target == null ||
                Stats == null ||
                _damageTarget == null ||
                _projectilePrefab == null ||
                Time.time < _nextAttackTime)
            {
                return;
            }

            Vector2 toTarget = Target.position - transform.position;

            if (toTarget.sqrMagnitude > _attackRange * _attackRange)
            {
                return;
            }

            _nextAttackTime = Time.time + Stats.AttackInterval;
            Vector3 spawnPosition = _muzzle != null
                ? _muzzle.position
                : transform.position;

            DamageProjectile projectile = Instantiate(
                _projectilePrefab,
                spawnPosition,
                Quaternion.identity);
            projectile.Launch(
                toTarget,
                Stats.AttackDamage,
                _projectileSpeed,
                _projectileLifetime,
                _damageTarget,
                gameObject);
        }

        private void OnValidate()
        {
            _attackRange = Mathf.Max(0f, _attackRange);
            _projectileSpeed = Mathf.Max(0f, _projectileSpeed);
            _projectileLifetime = Mathf.Max(0.05f, _projectileLifetime);
        }
    }
}
