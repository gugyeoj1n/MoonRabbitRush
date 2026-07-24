using UnityEngine;

namespace MoonRabbitRush.Combat
{
    public readonly struct DamageInfo
    {
        public DamageInfo(float amount, Vector2 hitPoint, GameObject source = null)
        {
            Amount = Mathf.Max(0f, amount);
            HitPoint = hitPoint;
            Source = source;
        }

        public float Amount { get; }
        public Vector2 HitPoint { get; }
        public GameObject Source { get; }
    }
}
