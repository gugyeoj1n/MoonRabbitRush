using System.Collections.Generic;
using UnityEngine;

namespace MoonRabbitRush.Weapons
{
    public sealed class OrbitingWeapon : WeaponBehaviour
    {
        [SerializeField] private OrbitingWeaponHitbox _hitboxPrefab;
        [SerializeField, Min(1f)] private float _activeSpeedMultiplier = 3f;
        [SerializeField, Min(0.1f)] private float _activeDuration = 5f;

        private readonly List<OrbitingWeaponHitbox> _hitboxes = new();
        private float _angle;
        private float _activeRemaining;
        private bool _isActiveSkillVisual;

        private void Update()
        {
            if (Owner == null || _hitboxes.Count == 0)
            {
                return;
            }

            _activeRemaining = Mathf.Max(
                0f,
                _activeRemaining - Time.deltaTime);
            bool isActive = _activeRemaining > 0f;
            SetActiveSkillVisual(isActive);
            float speedMultiplier =
                isActive ? _activeSpeedMultiplier : 1f;
            _angle = Mathf.Repeat(
                _angle + Stats.ProjectileSpeed * speedMultiplier * Time.deltaTime,
                360f);
            UpdateHitboxPositions();
        }

        protected override bool OnActivateActiveSkill()
        {
            _activeRemaining = _activeDuration;
            SetActiveSkillVisual(true);
            return true;
        }

        protected override void OnLevelChanged()
        {
            if (Owner == null || _hitboxPrefab == null)
            {
                return;
            }

            RebuildHitboxes();
        }

        protected override void OnInitialized()
        {
            if (_hitboxPrefab == null)
            {
                Debug.LogError("Orbiting weapon hitbox prefab is not assigned.", this);
                return;
            }

            RebuildHitboxes();
        }

        private void RebuildHitboxes()
        {
            int requiredCount = Mathf.Max(1, Stats.ProjectileCount);

            while (_hitboxes.Count > requiredCount)
            {
                int lastIndex = _hitboxes.Count - 1;
                PoolingManager.Release(
                    PoolType.WeaponShockDrone,
                    _hitboxes[lastIndex].gameObject);
                _hitboxes.RemoveAt(lastIndex);
            }

            while (_hitboxes.Count < requiredCount)
            {
                const PoolType poolType = PoolType.WeaponShockDrone;
                if (!PoolingManager.IsRegistered(poolType))
                {
                    PoolingManager.RegisterPool(
                        poolType,
                        () => Instantiate(_hitboxPrefab).gameObject,
                        defaultCapacity: 3,
                        maxSize: 10);
                }

                PoolingManager.GetObject(poolType, out GameObject hitboxObject);
                if (hitboxObject == null ||
                    !hitboxObject.TryGetComponent(
                        out OrbitingWeaponHitbox hitbox))
                {
                    break;
                }

                hitbox.transform.SetParent(transform, false);
                hitbox.transform.localPosition = Vector3.zero;
                _hitboxes.Add(hitbox);
            }

            foreach (OrbitingWeaponHitbox hitbox in _hitboxes)
            {
                hitbox.Configure(
                    Stats.Damage * Modifiers.DamageMultiplier,
                    Stats.Cooldown,
                    Owner.gameObject);
                hitbox.SetActiveSkillVisual(_activeRemaining > 0f);
            }

            UpdateHitboxPositions();
        }

        protected override void OnModifiersChanged()
        {
            RebuildHitboxes();
        }

        private void UpdateHitboxPositions()
        {
            if (_hitboxes.Count == 0)
            {
                return;
            }

            float angleStep = 360f / _hitboxes.Count;

            for (int index = 0; index < _hitboxes.Count; index++)
            {
                float angle = (_angle + angleStep * index) * Mathf.Deg2Rad;
                Vector2 offset = new(
                    Mathf.Cos(angle) * Stats.Range,
                    Mathf.Sin(angle) * Stats.Range);
                _hitboxes[index].MoveToLocal(offset);
            }
        }

        private void SetActiveSkillVisual(bool isActive)
        {
            if (_isActiveSkillVisual == isActive)
            {
                return;
            }

            _isActiveSkillVisual = isActive;

            foreach (OrbitingWeaponHitbox hitbox in _hitboxes)
            {
                hitbox.SetActiveSkillVisual(isActive);
            }
        }
    }
}
