using System;
using UnityEngine;

namespace MoonRabbitRush.Combat
{
    public static class DamageFeedbackEvents
    {
        public static event Action<float, Vector3> DamageApplied;

        public static void RaiseDamageApplied(float amount, Vector3 worldPosition)
        {
            if (amount > 0f)
            {
                DamageApplied?.Invoke(amount, worldPosition);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Clear()
        {
            DamageApplied = null;
        }
    }
}
