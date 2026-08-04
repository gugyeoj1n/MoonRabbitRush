using System;
using UnityEngine;

namespace MoonRabbitRush.Weapons
{
    [Serializable]
    public struct PassiveWeaponLevelStats
    {
        [SerializeField, Min(0f)] private float _damageBonus;
        [SerializeField, Min(0f)] private float _sizeBonus;
        [SerializeField, Min(0f)] private float _moveSpeedBonus;
        [SerializeField, Min(0)] private int _additionalWeaponCount;

        public float DamageBonus => _damageBonus;
        public float SizeBonus => _sizeBonus;
        public float MoveSpeedBonus => _moveSpeedBonus;
        public int AdditionalWeaponCount => _additionalWeaponCount;
    }
}
