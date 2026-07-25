using MoonRabbitRush.Player;
using UnityEngine;

namespace MoonRabbitRush.Progression
{
    [RequireComponent(typeof(PlayerExperience))]
    [RequireComponent(typeof(PlayerHealth))]
    public sealed class PlayerLootCollector : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float _lootRadius = 2.5f;

        private PlayerExperience _experience;
        private PlayerHealth _health;

        public float LootRadius => _lootRadius;
        public Vector2 Position => transform.position;
        public bool CanCollect => _health != null && _health.IsAlive;

        private void Awake()
        {
            _experience = GetComponent<PlayerExperience>();
            _health = GetComponent<PlayerHealth>();
        }

        public void CollectExperience(int amount)
        {
            _experience.AddExperience(amount);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.65f, 0f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, _lootRadius);
        }

        private void OnValidate()
        {
            _lootRadius = Mathf.Max(0.1f, _lootRadius);
        }
    }
}
