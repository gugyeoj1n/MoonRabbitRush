using MoonRabbitRush.Combat;
using MoonRabbitRush.Player;
using UnityEngine;

namespace MoonRabbitRush.Debugging
{
    /// <summary>
    /// Test-only damage source for manually verifying player damage and
    /// invincibility feedback. Do not use this component as production combat logic.
    /// </summary>
    [AddComponentMenu("Moon Rabbit Rush/Test Only/Damage Zone")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class TestDamageZone : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _damage = 20f;
        [SerializeField, Min(0.05f)] private float _damageInterval = 0.75f;

        private float _nextDamageTime;

        private void Reset()
        {
            Collider2D zoneCollider = GetComponent<Collider2D>();
            zoneCollider.isTrigger = true;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (Time.time < _nextDamageTime ||
                !other.TryGetComponent(out PlayerHealth playerHealth))
            {
                return;
            }

            _nextDamageTime = Time.time + _damageInterval;
            Vector2 hitPoint = other.ClosestPoint(transform.position);
            playerHealth.TakeDamage(new DamageInfo(_damage, hitPoint, gameObject));
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, transform.lossyScale);
        }
    }
}
