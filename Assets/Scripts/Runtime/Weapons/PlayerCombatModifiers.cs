using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoonRabbitRush.Weapons
{
    public sealed class PlayerCombatModifiers : MonoBehaviour
    {
        private readonly Dictionary<WeaponData, PassiveWeaponLevelStats> _sources =
            new();

        public float DamageMultiplier { get; private set; } = 1f;
        public float SizeMultiplier { get; private set; } = 1f;
        public float MoveSpeedMultiplier { get; private set; } = 1f;
        public int AdditionalWeaponCount { get; private set; }

        public event Action Changed;

        public static PlayerCombatModifiers GetOrAdd(GameObject owner)
        {
            PlayerCombatModifiers modifiers =
                owner.GetComponent<PlayerCombatModifiers>();
            return modifiers != null
                ? modifiers
                : owner.AddComponent<PlayerCombatModifiers>();
        }

        public void Set(WeaponData source, in PassiveWeaponLevelStats stats)
        {
            if (source == null)
            {
                return;
            }

            _sources[source] = stats;
            Recalculate();
        }

        private void Recalculate()
        {
            float damageBonus = 0f;
            float sizeBonus = 0f;
            float moveSpeedBonus = 0f;
            int additionalWeaponCount = 0;

            foreach (PassiveWeaponLevelStats stats in _sources.Values)
            {
                damageBonus += stats.DamageBonus;
                sizeBonus += stats.SizeBonus;
                moveSpeedBonus += stats.MoveSpeedBonus;
                additionalWeaponCount += stats.AdditionalWeaponCount;
            }

            DamageMultiplier = 1f + damageBonus;
            SizeMultiplier = 1f + sizeBonus;
            MoveSpeedMultiplier = 1f + moveSpeedBonus;
            AdditionalWeaponCount = additionalWeaponCount;
            Changed?.Invoke();
        }
    }
}
