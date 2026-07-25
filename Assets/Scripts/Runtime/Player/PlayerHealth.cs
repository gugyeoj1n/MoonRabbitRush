using System;
using MoonRabbitRush.Combat;
using UnityEngine;

namespace MoonRabbitRush.Player
{
    public sealed class PlayerHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private PlayerStatsData _stats;
        [SerializeField] private PlayerMovement _movement;

        private float _currentHealth;
        private float _invincibleUntil;
        private bool _isInitialized;

        public event Action<float, float> HealthChanged;
        public event Action<float> Damaged;
        public event Action Died;

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _stats != null ? _stats.MaxHealth : 0f;
        public bool IsAlive => _isInitialized && _currentHealth > 0f;
        public bool IsInvincible => IsAlive && Time.time < _invincibleUntil;

        private void Awake()
        {
            if (_stats == null)
            {
                Debug.LogError($"{nameof(PlayerStatsData)} is not assigned.", this);
                return;
            }

            if (_movement == null)
            {
                _movement = GetComponent<PlayerMovement>();
            }

            _currentHealth = _stats.MaxHealth;
            _isInitialized = true;
        }

        private void Start()
        {
            if (_isInitialized)
            {
                HealthChanged?.Invoke(_currentHealth, _stats.MaxHealth);
            }
        }

        private void Update()
        {
            if (!IsAlive || _stats.RecoveryPerSecond <= 0f)
            {
                return;
            }

            Heal(_stats.RecoveryPerSecond * Time.deltaTime);
        }

        public void TakeDamage(in DamageInfo damage)
        {
            if (!IsAlive || IsInvincible || damage.Amount <= 0f)
            {
                return;
            }

            float appliedDamage = Mathf.Max(1f, damage.Amount - _stats.Defense);
            _currentHealth = Mathf.Max(0f, _currentHealth - appliedDamage);
            _invincibleUntil = Time.time + _stats.InvincibilityDuration;

            Damaged?.Invoke(appliedDamage);
            DamageFeedbackEvents.RaiseDamageApplied(
                appliedDamage,
                transform.position,
                true);
            HealthChanged?.Invoke(_currentHealth, _stats.MaxHealth);
            Debug.Log(
                $"[PlayerHealth] HP: {_currentHealth:0.##}/{_stats.MaxHealth:0.##}",
                this);

            if (_currentHealth <= 0f)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (!IsAlive || amount <= 0f || _currentHealth >= _stats.MaxHealth)
            {
                return;
            }

            _currentHealth = Mathf.Min(_stats.MaxHealth, _currentHealth + amount);
            HealthChanged?.Invoke(_currentHealth, _stats.MaxHealth);
        }

        private void Die()
        {
            _invincibleUntil = 0f;
            _movement?.SetMovementEnabled(false);
            Debug.Log("[PlayerHealth] Player died.", this);
            Died?.Invoke();
        }
    }
}
