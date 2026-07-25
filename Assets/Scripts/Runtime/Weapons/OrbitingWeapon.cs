using System.Collections.Generic;
using UnityEngine;

namespace MoonRabbitRush.Weapons
{
    public sealed class OrbitingWeapon : WeaponBehaviour
    {
        [SerializeField] private OrbitingWeaponHitbox _hitboxPrefab;

        private readonly List<OrbitingWeaponHitbox> _hitboxes = new();
        private float _angle;

        private void Update()
        {
            if (Owner == null || _hitboxes.Count == 0)
            {
                return;
            }

            _angle = Mathf.Repeat(
                _angle + Stats.ProjectileSpeed * Time.deltaTime,
                360f);

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
                Destroy(_hitboxes[lastIndex].gameObject);
                _hitboxes.RemoveAt(lastIndex);
            }

            while (_hitboxes.Count < requiredCount)
            {
                OrbitingWeaponHitbox hitbox = Instantiate(
                    _hitboxPrefab,
                    transform);
                hitbox.transform.localPosition = Vector3.zero;
                _hitboxes.Add(hitbox);
            }

            foreach (OrbitingWeaponHitbox hitbox in _hitboxes)
            {
                hitbox.Configure(Stats.Damage, Stats.Cooldown, Owner.gameObject);
            }
        }

    }
}
