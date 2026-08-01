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

        private int _lastRemainingEnemyCount = -1;

        private void Awake()
        {
            ResolveReferences();
            PublishAll();
        }

        private void OnEnable()
        {
            ResolveReferences();

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
            }

            PublishAll();
        }

        private void Start()
        {
            PublishAll();
        }

        private void Update()
        {
            if (_waveDirector == null)
            {
                return;
            }

            int remainingEnemyCount = _waveDirector.RemainingEnemyCount;
            if (remainingEnemyCount == _lastRemainingEnemyCount)
            {
                return;
            }

            _lastRemainingEnemyCount = remainingEnemyCount;
            DataBindingManager.SetValue(
                Property.MonsterRemain,
                remainingEnemyCount);
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
            }
        }

        private void ResolveReferences()
        {
            _playerHealth ??= FindAnyObjectByType<PlayerHealth>();
            _playerExperience ??= FindAnyObjectByType<PlayerExperience>();
            _waveDirector ??= FindAnyObjectByType<WaveDirector>();
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

                _lastRemainingEnemyCount =
                    _waveDirector.RemainingEnemyCount;
                DataBindingManager.SetValue(
                    Property.MonsterRemain,
                    _lastRemainingEnemyCount);
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
            _lastRemainingEnemyCount = -1;
        }

        private void HandleWaveCompleted(int wave)
        {
            DataBindingManager.SetValue(Property.MonsterRemain, 0);
            _lastRemainingEnemyCount = 0;
        }
    }
}
