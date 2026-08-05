using System;
using MoonRabbitRush.Combat;
using UnityEngine;

namespace MoonRabbitRush.Defense
{
    public sealed class MoonBaseHealth : MonoBehaviour, IDamageable
    {
        [SerializeField, Min(1f)] private float _maxHealth = 300f;

        [Header("Damage Sprites")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Sprite _healthySprite;
        [SerializeField] private Sprite _damagedSprite;
        [SerializeField] private Sprite _criticalSprite;
        [SerializeField] private Sprite _destroyedSprite;

        private float _currentHealth;
        private bool _isInitialized;

        public float CurrentHealth
        {
            get
            {
                EnsureInitialized();
                return _currentHealth;
            }
        }
        public float MaxHealth => _maxHealth;
        public bool IsAlive
        {
            get
            {
                EnsureInitialized();
                return _currentHealth > 0f;
            }
        }

        public event Action<float, float> HealthChanged;
        public event Action<float> Damaged;
        public event Action Destroyed;

        private void Awake()
        {
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (_isInitialized)
            {
                return;
            }

            _spriteRenderer ??= GetComponent<SpriteRenderer>();
            _healthySprite ??= _spriteRenderer != null
                ? _spriteRenderer.sprite
                : null;
            _currentHealth = _maxHealth;
            _isInitialized = true;
            RefreshSprite();
        }

        private void Start()
        {
            HealthChanged?.Invoke(_currentHealth, _maxHealth);
        }

        public void TakeDamage(in DamageInfo damage)
        {
            EnsureInitialized();

            if (!IsAlive || damage.Amount <= 0f)
            {
                return;
            }

            float appliedDamage = Mathf.Min(_currentHealth, damage.Amount);
            _currentHealth -= appliedDamage;
            Damaged?.Invoke(appliedDamage);
            HealthChanged?.Invoke(_currentHealth, _maxHealth);
            DamageFeedbackEvents.RaiseDamageApplied(
                appliedDamage,
                damage.HitPoint,
                true);
            RefreshSprite();

            if (_currentHealth <= 0f)
            {
                Debug.Log("[MoonBaseHealth] Moon base was destroyed.", this);
                Destroyed?.Invoke();
            }
        }

        private void RefreshSprite()
        {
            if (_spriteRenderer == null)
            {
                return;
            }

            float ratio = _maxHealth <= 0f ? 0f : _currentHealth / _maxHealth;
            Sprite nextSprite = ratio switch
            {
                <= 0f when _destroyedSprite != null => _destroyedSprite,
                <= 0.3f when _criticalSprite != null => _criticalSprite,
                <= 0.6f when _damagedSprite != null => _damagedSprite,
                _ => _healthySprite
            };

            if (nextSprite != null)
            {
                _spriteRenderer.sprite = nextSprite;
            }
        }
    }
}
