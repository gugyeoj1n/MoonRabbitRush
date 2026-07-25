using System;
using MoonRabbitRush.Combat;
using UnityEngine;

namespace MoonRabbitRush.Enemies
{
    public sealed class EnemyHealth : MonoBehaviour, IDamageable
    {
        private EnemyStatsData _stats;
        private float _currentHealth;
        private bool _isInitialized;

        public event Action<float, float> HealthChanged;
        public event Action<float> Damaged;
        public event Action<DamageInfo> DamageReceived;
        public event Action Died;

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _stats != null ? _stats.MaxHealth : 0f;
        public bool IsAlive => _isInitialized && _currentHealth > 0f;

        public void Initialize(EnemyStatsData stats)
        {
            _stats = stats;

            if (_stats == null)
            {
                Debug.LogError($"{nameof(EnemyStatsData)} is not assigned.", this);
                return;
            }

            _isInitialized = true;
            ResetHealth();
        }

        public void ResetHealth()
        {
            if (!_isInitialized)
            {
                return;
            }

            _currentHealth = _stats.MaxHealth;
            HealthChanged?.Invoke(_currentHealth, _stats.MaxHealth);
        }

        public void TakeDamage(in DamageInfo damage)
        {
            if (!IsAlive || damage.Amount <= 0f)
            {
                return;
            }

            float appliedDamage = Mathf.Max(1f, damage.Amount - _stats.Defense);
            _currentHealth = Mathf.Max(0f, _currentHealth - appliedDamage);

            Damaged?.Invoke(appliedDamage);
            DamageReceived?.Invoke(damage);
            DamageFeedbackEvents.RaiseDamageApplied(
                appliedDamage,
                transform.position,
                false);
            HealthChanged?.Invoke(_currentHealth, _stats.MaxHealth);
            Debug.Log(
                $"[EnemyHealth] HP: {_currentHealth:0.##}/{_stats.MaxHealth:0.##}",
                this);

            if (_currentHealth <= 0f)
            {
                Debug.Log("[EnemyHealth] Enemy died.", this);
                Died?.Invoke();
            }
        }
    }
}
