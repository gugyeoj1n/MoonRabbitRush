using MoonRabbitRush.Enemies;
using UnityEngine;

namespace MoonRabbitRush.Weapons
{
    public sealed class HomingMissileWeapon : WeaponBehaviour
    {
        [SerializeField] private HomingWeaponProjectile _projectilePrefab;
        [SerializeField, Range(0f, 180f)] private float _spreadAngle = 20f;

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

            Fire(target);
            _cooldownRemaining = Stats.Cooldown;
        }

        protected override void OnLevelChanged()
        {
            _cooldownRemaining = Mathf.Min(_cooldownRemaining, Stats.Cooldown);
        }

        private void Fire(EnemyHealth target)
        {
            int projectileCount = Mathf.Max(1, Stats.ProjectileCount);

            for (int index = 0; index < projectileCount; index++)
            {
                Vector2 targetDirection =
                    (target.transform.position - Owner.position).normalized;
                float offset = GetSpreadOffset(index, projectileCount);
                Vector2 launchDirection =
                    Quaternion.Euler(0f, 0f, offset) * targetDirection;

                HomingWeaponProjectile projectile = Instantiate(
                    _projectilePrefab,
                    Owner.position,
                    Quaternion.identity);
                projectile.Launch(
                    launchDirection,
                    target,
                    Stats,
                    Owner.gameObject);
            }
        }

        private float GetSpreadOffset(int index, int count)
        {
            if (count <= 1)
            {
                return 0f;
            }

            return Mathf.Lerp(
                -_spreadAngle * 0.5f,
                _spreadAngle * 0.5f,
                index / (float)(count - 1));
        }
    }
}
