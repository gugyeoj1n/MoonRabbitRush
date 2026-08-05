using MoonRabbitRush.Defense;
using MoonRabbitRush.Player;
using MoonRabbitRush.Progression;
using MoonRabbitRush.Waves;
using UnityEngine;

namespace MoonRabbitRush.UI
{
    public sealed class GameplayHudDataBinder : MonoBehaviour
    {
        [SerializeField] private Transform _playerRoot;
        [SerializeField] private WaveDirector _waveDirector;

        private PlayerHealth _playerHealth;
        private PlayerExperience _playerExperience;
        private MoonBaseHealth _baseHealth;

        private void Awake()
        {
            ResolveReferences();
            PublishAll();
        }

        private void OnEnable()
        {
            if (_playerHealth != null)
            {
                _playerHealth.HealthChanged += HandleHealthChanged;
            }

            if (_playerExperience != null)
            {
                _playerExperience.ExperienceChanged += HandleExperienceChanged;
            }

            if (_baseHealth != null)
            {
                _baseHealth.HealthChanged += HandleBaseHealthChanged;
            }

            if (_waveDirector != null)
            {
                _waveDirector.WaveStarted += HandleWaveStarted;
                _waveDirector.WaveCompleted += HandleWaveCompleted;
                _waveDirector.RemainingEnemyCountChanged += HandleRemainingEnemyCountChanged;
            }

            PublishAll();
        }

        private void OnDisable()
        {
            if (_playerHealth != null)
            {
                _playerHealth.HealthChanged -= HandleHealthChanged;
            }

            if (_playerExperience != null)
            {
                _playerExperience.ExperienceChanged -= HandleExperienceChanged;
            }

            if (_baseHealth != null)
            {
                _baseHealth.HealthChanged -= HandleBaseHealthChanged;
            }

            if (_waveDirector != null)
            {
                _waveDirector.WaveStarted -= HandleWaveStarted;
                _waveDirector.WaveCompleted -= HandleWaveCompleted;
                _waveDirector.RemainingEnemyCountChanged -= HandleRemainingEnemyCountChanged;
            }

        }

        private void ResolveReferences()
        {
            if (_playerRoot != null)
            {
                _playerHealth ??= _playerRoot.GetComponent<PlayerHealth>();
                _playerExperience ??=
                    _playerRoot.GetComponent<PlayerExperience>();
            }

            _baseHealth ??= FindAnyObjectByType<MoonBaseHealth>();
        }

        private void PublishAll()
        {
            if (_playerHealth != null)
            {
                HandleHealthChanged(
                    _playerHealth.CurrentHealth,
                    _playerHealth.MaxHealth);
            }

            if (_playerExperience != null)
            {
                HandleExperienceChanged(
                    _playerExperience.CurrentExperience,
                    _playerExperience.RequiredExperience,
                    _playerExperience.CurrentLevel);
            }

            if (_baseHealth != null)
            {
                HandleBaseHealthChanged(
                    _baseHealth.CurrentHealth,
                    _baseHealth.MaxHealth);
            }

            if (_waveDirector != null)
            {
                DataBindingManager.SetValue(
                    Property.Wave,
                    _waveDirector.CurrentWave);
                DataBindingManager.SetValue(
                    Property.MonsterRemain,
                    _waveDirector.RemainingEnemyCount);
            }
        }

        private static void HandleHealthChanged(
            float currentHealth,
            float maxHealth)
        {
            DataBindingManager.SetValue(
                Property.PlayerHealth,
                Mathf.CeilToInt(currentHealth));
            DataBindingManager.SetValue(
                Property.PlayerMaxHealth,
                Mathf.CeilToInt(maxHealth));
        }

        private static void HandleExperienceChanged(
            int currentExperience,
            int requiredExperience,
            int currentLevel)
        {
            DataBindingManager.SetValue(
                Property.PlayerExperience,
                currentExperience);
            DataBindingManager.SetValue(
                Property.PlayerMaxExperience,
                requiredExperience);
            DataBindingManager.SetValue(
                Property.PlayerLevel,
                currentLevel);
        }

        private static void HandleBaseHealthChanged(
            float currentHealth,
            float maxHealth)
        {
            DataBindingManager.SetValue(
                Property.BaseHealth,
                Mathf.CeilToInt(currentHealth));
            DataBindingManager.SetValue(
                Property.BaseMaxHealth,
                Mathf.CeilToInt(maxHealth));
        }

        private void HandleWaveStarted(int wave)
        {
            DataBindingManager.SetValue(Property.Wave, wave);
            DataBindingManager.SetValue(
                Property.MonsterRemain,
                _waveDirector != null ? _waveDirector.RemainingEnemyCount : 0);
        }

        private void HandleWaveCompleted(int wave)
        {
            DataBindingManager.SetValue(Property.MonsterRemain, 0);
        }

        private void HandleRemainingEnemyCountChanged(int remainingEnemyCount)
        {
            DataBindingManager.SetValue(
                Property.MonsterRemain,
                remainingEnemyCount);
        }
    }
}
