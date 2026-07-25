using System;
using UnityEngine;

namespace MoonRabbitRush.Weapons
{
    [Serializable]
    public struct WeaponLevelStats
    {
        [SerializeField, Min(0f)] private float _damage;
        [SerializeField, Min(0.01f)] private float _cooldown;
        [SerializeField, Min(1)] private int _projectileCount;
        [SerializeField, Min(0f)] private float _projectileSpeed;
        [SerializeField, Min(0f)] private float _range;
        [SerializeField, Min(0.05f)] private float _duration;
        [SerializeField, Min(0)] private int _pierceCount;
        [SerializeField, Min(0f)] private float _areaRadius;

        public float Damage => _damage;
        public float Cooldown => _cooldown;
        public int ProjectileCount => _projectileCount;
        public float ProjectileSpeed => _projectileSpeed;
        public float Range => _range;
        public float Duration => _duration;
        public int PierceCount => _pierceCount;
        public float AreaRadius => _areaRadius;
    }
}
