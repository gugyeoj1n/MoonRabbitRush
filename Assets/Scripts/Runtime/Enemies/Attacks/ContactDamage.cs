using MoonRabbitRush.Combat;
using UnityEngine;

namespace MoonRabbitRush.Enemies
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class ContactDamage : EnemyBehaviour
    {
        private float _nextAttackTime;

        public override void Initialize(Transform target, EnemyStatsData stats)
        {
            base.Initialize(target, stats);
            _nextAttackTime = 0f;
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (Stats == null || Time.time < _nextAttackTime)
            {
                return;
            }

            Component damageTarget =
                collision.collider.GetComponentInParent(typeof(IDamageable));
            IDamageable damageable = damageTarget as IDamageable;

            if (damageable == null ||
                damageTarget is EnemyHealth ||
                !damageable.IsAlive)
            {
                return;
            }

            _nextAttackTime = Time.time + Stats.AttackInterval;
            Vector2 hitPoint = collision.contactCount > 0
                ? collision.GetContact(0).point
                : collision.transform.position;

            damageable.TakeDamage(
                new DamageInfo(Stats.AttackDamage, hitPoint, gameObject));
        }
    }
}
