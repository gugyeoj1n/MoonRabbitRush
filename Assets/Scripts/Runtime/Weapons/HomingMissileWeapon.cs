using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MoonRabbitRush.Enemies;
using UnityEngine;

namespace MoonRabbitRush.Weapons
{
    public sealed class HomingMissileWeapon : WeaponBehaviour
    {
        [SerializeField] private HomingWeaponProjectile _projectilePrefab;
        [SerializeField, Range(0f, 180f)] private float _spreadAngle = 20f;
        [SerializeField, Min(1)] private int _activeMissileCount = 10;
        [SerializeField, Min(0.01f)] private float _activeFireInterval = 0.08f;

        private float _cooldownRemaining;
        private CancellationTokenSource _activeBurstCts;

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

        protected override bool OnActivateActiveSkill()
        {
            if (EnemyRegistry.FindClosest(Owner.position, Stats.Range) == null)
            {
                return false;
            }

            CancelActiveBurst();
            _activeBurstCts = new CancellationTokenSource();
            FireActiveBurstAsync(_activeBurstCts.Token).Forget();
            return true;
        }

        private async UniTaskVoid FireActiveBurstAsync(
            CancellationToken cancellationToken)
        {
            try
            {
                for (int index = 0; index < _activeMissileCount; index++)
                {
                    EnemyHealth target = EnemyRegistry.FindClosest(
                        Owner.position,
                        Stats.Range);

                    if (target != null)
                    {
                        FireSingle(target, Vector2.zero);
                    }

                    await UniTask.Delay(
                        TimeSpan.FromSeconds(_activeFireInterval),
                        DelayType.DeltaTime,
                        PlayerLoopTiming.Update,
                        cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void Fire(EnemyHealth target)
        {
            int projectileCount = Mathf.Max(
                1,
                Stats.ProjectileCount + Modifiers.AdditionalWeaponCount);

            for (int index = 0; index < projectileCount; index++)
            {
                Vector2 targetDirection =
                    (target.transform.position - Owner.position).normalized;
                float offset = GetSpreadOffset(index, projectileCount);
                Vector2 launchDirection =
                    Quaternion.Euler(0f, 0f, offset) * targetDirection;

                FireSingle(target, launchDirection);
            }
        }

        private void FireSingle(EnemyHealth target, Vector2 launchDirection)
        {
            if (launchDirection == Vector2.zero)
            {
                launchDirection =
                    (target.transform.position - Owner.position).normalized;
            }

            const PoolType poolType = PoolType.ProjectileCarrotMissile;
            if (!PoolingManager.IsRegistered(poolType))
            {
                PoolingManager.RegisterPool(
                    poolType,
                    () => Instantiate(_projectilePrefab).gameObject,
                    defaultCapacity: 10,
                    maxSize: 100);
            }

            PoolingManager.GetObject(poolType, out GameObject projectileObject);
            if (projectileObject == null ||
                !projectileObject.TryGetComponent(
                    out HomingWeaponProjectile projectile))
            {
                return;
            }

            projectile.transform.SetPositionAndRotation(
                Owner.position,
                Quaternion.identity);
            projectile.Launch(
                launchDirection,
                target,
                Stats,
                Stats.Damage * Modifiers.DamageMultiplier,
                Modifiers.SizeMultiplier,
                Owner.gameObject);
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

        private void OnDisable()
        {
            CancelActiveBurst();
        }

        private void CancelActiveBurst()
        {
            if (_activeBurstCts == null)
            {
                return;
            }

            _activeBurstCts.Cancel();
            _activeBurstCts.Dispose();
            _activeBurstCts = null;
        }
    }
}
