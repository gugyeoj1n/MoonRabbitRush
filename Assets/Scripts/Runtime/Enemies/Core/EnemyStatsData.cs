using UnityEngine;

namespace MoonRabbitRush.Enemies
{
    [CreateAssetMenu(
        fileName = "SO_Enemy_New",
        menuName = "Moon Rabbit Rush/Enemy/Stats")]
    public sealed class EnemyStatsData : ScriptableObject
    {
        [Header("Survival")]
        [SerializeField, Min(1f)] private float _maxHealth = 50f;
        [SerializeField, Min(0f)] private float _defense;

        [Header("Movement")]
        [SerializeField, Min(0f)] private float _moveSpeed = 2f;

        [Header("Attack")]
        [SerializeField, Min(0f)] private float _attackDamage = 10f;
        [SerializeField, Min(0.05f)] private float _attackInterval = 0.75f;

        [Header("Rewards")]
        [SerializeField, Min(1)] private int _experienceReward = 1;
        [SerializeField, Min(0)] private int _scoreReward = 0;

        public float MaxHealth => _maxHealth;
        public float Defense => _defense;
        public float MoveSpeed => _moveSpeed;
        public float AttackDamage => _attackDamage;
        public float AttackInterval => _attackInterval;
        public int ExperienceReward => _experienceReward;
        public int ScoreReward =>
            _scoreReward > 0 ? _scoreReward : _experienceReward * 10;

        private void OnValidate()
        {
            _maxHealth = Mathf.Max(1f, _maxHealth);
            _defense = Mathf.Max(0f, _defense);
            _moveSpeed = Mathf.Max(0f, _moveSpeed);
            _attackDamage = Mathf.Max(0f, _attackDamage);
            _attackInterval = Mathf.Max(0.05f, _attackInterval);
            _experienceReward = Mathf.Max(1, _experienceReward);
            _scoreReward = Mathf.Max(0, _scoreReward);
        }
    }
}
