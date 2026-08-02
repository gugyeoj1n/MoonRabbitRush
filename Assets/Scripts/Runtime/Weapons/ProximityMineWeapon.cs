using System.Collections.Generic;
using UnityEngine;

namespace MoonRabbitRush.Weapons
{
    public sealed class ProximityMineWeapon : WeaponBehaviour
    {
        [Header("Mine")]
        [SerializeField] private ProximityMine _minePrefab;

        [Header("Throw")]
        [SerializeField, Min(0.05f)] private float _throwDuration = 0.55f;
        [SerializeField, Min(0f)] private float _arcHeight = 1.4f;
        [SerializeField, Range(0.1f, 1f)] private float _minimumDistanceRatio = 0.75f;
        [SerializeField] private bool _useRandomDirections = true;

        private float _cooldownRemaining;
        private readonly List<ProximityMine> _spawnedMines = new();

        private void Update()
        {
            if (Owner == null || _minePrefab == null)
            {
                return;
            }

            _cooldownRemaining -= Time.deltaTime;

            if (_cooldownRemaining > 0f)
            {
                return;
            }

            ThrowMines();
            _cooldownRemaining = Stats.Cooldown;
        }

        protected override void OnLevelChanged()
        {
            _cooldownRemaining = Mathf.Min(_cooldownRemaining, Stats.Cooldown);
        }

        private void ThrowMines()
        {
            int mineCount = Mathf.Max(1, Stats.ProjectileCount);
            float baseAngle = Random.Range(0f, 360f);

            for (int index = 0; index < mineCount; index++)
            {
                float angle = _useRandomDirections
                    ? Random.Range(0f, 360f)
                    : baseAngle + 360f * index / mineCount;
                Vector2 direction = new(
                    Mathf.Cos(angle * Mathf.Deg2Rad),
                    Mathf.Sin(angle * Mathf.Deg2Rad));
                float distance = Stats.ProjectileSpeed * _throwDuration *
                    Random.Range(_minimumDistanceRatio, 1f);

                const PoolType poolType = PoolType.WeaponSpaceCarrotMine;
                if (!PoolingManager.IsRegistered(poolType))
                {
                    PoolingManager.RegisterPool(
                        poolType,
                        () => Instantiate(_minePrefab).gameObject,
                        defaultCapacity: 10,
                        maxSize: 100);
                }

                PoolingManager.GetObject(poolType, out GameObject mineObject);
                if (mineObject == null ||
                    !mineObject.TryGetComponent(out ProximityMine mine))
                {
                    continue;
                }

                mine.transform.SetPositionAndRotation(
                    Owner.position,
                    Quaternion.identity);
                mine.Launch(
                    (Vector2)Owner.position + direction * distance,
                    _throwDuration,
                    _arcHeight,
                    Stats,
                    Owner.gameObject);
                _spawnedMines.Add(mine);
            }
        }

        protected override bool OnActivateActiveSkill()
        {
            _spawnedMines.RemoveAll(
                mine => mine == null || !mine.IsActive);
            bool detonated = false;

            foreach (ProximityMine mine in _spawnedMines)
            {
                detonated |= mine.TryForceDetonate();
            }

            _spawnedMines.RemoveAll(
                mine => mine == null || !mine.IsActive);
            return detonated;
        }

        private void OnValidate()
        {
            _throwDuration = Mathf.Max(0.05f, _throwDuration);
            _arcHeight = Mathf.Max(0f, _arcHeight);
            _minimumDistanceRatio = Mathf.Clamp(_minimumDistanceRatio, 0.1f, 1f);
        }
    }
}
