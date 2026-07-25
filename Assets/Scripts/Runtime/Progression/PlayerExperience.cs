using System;
using UnityEngine;

namespace MoonRabbitRush.Progression
{
    public sealed class PlayerExperience : MonoBehaviour
    {
        [SerializeField] private ExperienceTableData _experienceTable;

        private int _currentLevel = 1;
        private int _currentExperience;

        public event Action<int, int, int> ExperienceChanged;
        public event Action<int> LeveledUp;

        public int CurrentLevel => _currentLevel;
        public int CurrentExperience => _currentExperience;
        public int RequiredExperience =>
            _experienceTable != null &&
            _experienceTable.TryGetRequiredExperience(
                _currentLevel,
                out int requiredExperience)
                ? requiredExperience
                : 0;

        private void Start()
        {
            if (_experienceTable == null)
            {
                Debug.LogError(
                    $"{nameof(ExperienceTableData)} is not assigned.",
                    this);
                return;
            }

            ExperienceChanged?.Invoke(
                _currentExperience,
                RequiredExperience,
                _currentLevel);
        }

        public void AddExperience(int amount)
        {
            if (amount <= 0 || _experienceTable == null)
            {
                return;
            }

            _currentExperience += amount;

            while (_experienceTable.TryGetRequiredExperience(
                _currentLevel,
                out int requiredExperience) &&
                _currentExperience >= requiredExperience)
            {
                _currentExperience -= requiredExperience;
                _currentLevel++;
                Debug.Log(
                    $"[PlayerExperience] Level Up! {_currentLevel}",
                    this);
                LeveledUp?.Invoke(_currentLevel);
            }

            ExperienceChanged?.Invoke(
                _currentExperience,
                RequiredExperience,
                _currentLevel);
        }
    }
}
