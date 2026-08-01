using MoonRabbitRush.Player;
using MoonRabbitRush.Progression;
using MoonRabbitRush.Waves;
using UnityEngine;

namespace MoonRabbitRush.UI
{
    public sealed class GameplayHudDataBinder : MonoBehaviour
    {
        [SerializeField] private PlayerHealth _playerHealth;
        [SerializeField] private PlayerExperience _playerExperience;
        [SerializeField] private WaveDirector _waveDirector;

        private void Awake()
        {
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

            if (_waveDirector != null)
            {
                _waveDirector.WaveStarted -= HandleWaveStarted;
                _waveDirector.WaveCompleted -= HandleWaveCompleted;
                _waveDirector.RemainingEnemyCountChanged -= HandleRemainingEnemyCountChanged;
            }
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
