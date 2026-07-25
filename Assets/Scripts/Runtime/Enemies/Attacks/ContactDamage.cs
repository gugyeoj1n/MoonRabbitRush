using MoonRabbitRush.Combat;
using MoonRabbitRush.Player;
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

            PlayerHealth playerHealth =
                collision.collider.GetComponentInParent<PlayerHealth>();

            if (playerHealth == null || !playerHealth.IsAlive)
            {
                return;
            }

            _nextAttackTime = Time.time + Stats.AttackInterval;
            Vector2 hitPoint = collision.contactCount > 0
                ? collision.GetContact(0).point
                : collision.transform.position;

            playerHealth.TakeDamage(
                new DamageInfo(Stats.AttackDamage, hitPoint, gameObject));
        }
    }
}
