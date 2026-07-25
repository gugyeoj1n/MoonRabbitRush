using System;
using UnityEngine;

namespace MoonRabbitRush.Combat
{
    public static class DamageFeedbackEvents
    {
        public static event Action<float, Vector3, bool> DamageApplied;

        public static void RaiseDamageApplied(
            float amount,
            Vector3 worldPosition,
            bool isPlayer)
        {
            if (amount > 0f)
            {
                DamageApplied?.Invoke(amount, worldPosition, isPlayer);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Clear()
        {
            DamageApplied = null;
        }
    }
}
