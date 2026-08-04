using MoonRabbitRush.Enemies;
using UnityEngine;

namespace MoonRabbitRush.Weapons
{
    public sealed class CrescentBoomerangWeapon : WeaponBehaviour
    {
        private const int ActiveBoomerangCount = 8;

        [SerializeField] private CrescentBoomerangProjectile _projectilePrefab;
        [SerializeField, Range(0f, 180f)] private float _spreadAngle = 24f;

        private float _cooldownRemaining;

        private void Update()
        {
            if (Owner == null || _projectilePrefab == null)
            {
                return;
            }

            _cooldownRemaining -= Time.deltaTime;
            if (_cooldownRemaining > 0f)
            {
                return;
            }

            EnemyHealth target = EnemyRegistry.FindClosest(
                Owner.position,
                Stats.Range);
            if (target == null)
            {
                return;
            }

            FireVolley(
                (target.transform.position - Owner.position).normalized);
            _cooldownRemaining = Stats.Cooldown;
        }

        protected override void OnLevelChanged()
        {
            _cooldownRemaining = Mathf.Min(_cooldownRemaining, Stats.Cooldown);
        }

        protected override bool OnActivateActiveSkill()
        {
            if (Owner == null || _projectilePrefab == null)
            {
                return false;
            }

            for (int index = 0; index < ActiveBoomerangCount; index++)
            {
                float angle = 360f * index / ActiveBoomerangCount;
                Vector2 direction = new(
                    Mathf.Cos(angle * Mathf.Deg2Rad),
                    Mathf.Sin(angle * Mathf.Deg2Rad));
                Spawn(direction);
            }

            return true;
        }

        private void FireVolley(Vector2 direction)
        {
            int count = Mathf.Max(
                1,
                Stats.ProjectileCount + Modifiers.AdditionalWeaponCount);

            for (int index = 0; index < count; index++)
            {
                float offset = count <= 1
                    ? 0f
                    : Mathf.Lerp(
                        -_spreadAngle * 0.5f,
                        _spreadAngle * 0.5f,
                        index / (float)(count - 1));
                Vector2 launchDirection =
                    Quaternion.Euler(0f, 0f, offset) * direction;
                Spawn(launchDirection);
            }
        }

        private void Spawn(Vector2 direction)
        {
            const PoolType poolType = PoolType.ProjectileCrescentBoomerang;
            if (!PoolingManager.IsRegistered(poolType))
            {
                PoolingManager.RegisterPool(
                    poolType,
                    () => Instantiate(_projectilePrefab).gameObject,
                    defaultCapacity: 12,
                    maxSize: 100);
            }

            PoolingManager.GetObject(poolType, out GameObject projectileObject);
            if (projectileObject == null ||
                !projectileObject.TryGetComponent(
                    out CrescentBoomerangProjectile projectile))
            {
                return;
            }

            projectile.transform.SetPositionAndRotation(
                Owner.position,
                Quaternion.identity);
            projectile.Launch(
                Owner,
                direction,
                Stats.Damage * Modifiers.DamageMultiplier,
                Stats.ProjectileSpeed,
                Stats.Range,
                Stats.Duration,
                Modifiers.SizeMultiplier,
                Owner.gameObject);
        }
    }
}
