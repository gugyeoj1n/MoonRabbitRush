using UnityEngine;

namespace MoonRabbitRush.Player
{
    [CreateAssetMenu(
        fileName = "SO_Character_New",
        menuName = "Moon Rabbit Rush/Player/Stats")]
    public sealed class PlayerStatsData : ScriptableObject
    {
        [Header("Movement")]
        [SerializeField, Min(0f)] private float _moveSpeed = 5f;

        [Header("Survival")]
        [SerializeField, Min(1f)] private float _maxHealth = 100f;
        [SerializeField, Min(0f)] private float _defense;
        [SerializeField, Min(0f)] private float _recoveryPerSecond;
        [SerializeField, Min(0f)] private float _invincibilityDuration = 0.5f;

        public float MoveSpeed => _moveSpeed;
        public float MaxHealth => _maxHealth;
        public float Defense => _defense;
        public float RecoveryPerSecond => _recoveryPerSecond;
        public float InvincibilityDuration => _invincibilityDuration;

        private void OnValidate()
        {
            _moveSpeed = Mathf.Max(0f, _moveSpeed);
            _maxHealth = Mathf.Max(1f, _maxHealth);
            _defense = Mathf.Max(0f, _defense);
            _recoveryPerSecond = Mathf.Max(0f, _recoveryPerSecond);
            _invincibilityDuration = Mathf.Max(0f, _invincibilityDuration);
        }
    }
}
