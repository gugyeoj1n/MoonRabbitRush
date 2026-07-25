using UnityEngine;

namespace MoonRabbitRush.Progression
{
    [CreateAssetMenu(
        fileName = "SO_ExperienceTable",
        menuName = "Moon Rabbit Rush/Progression/Experience Table")]
    public sealed class ExperienceTableData : ScriptableObject
    {
        [SerializeField] private int[] _requiredExperienceByLevel;

        public int MaxLevel => (_requiredExperienceByLevel?.Length ?? 0) + 1;

        public bool TryGetRequiredExperience(
            int currentLevel,
            out int requiredExperience)
        {
            int index = currentLevel - 1;

            if (_requiredExperienceByLevel == null ||
                index < 0 ||
                index >= _requiredExperienceByLevel.Length)
            {
                requiredExperience = 0;
                return false;
            }

            requiredExperience = Mathf.Max(1, _requiredExperienceByLevel[index]);
            return true;
        }

        private void OnValidate()
        {
            if (_requiredExperienceByLevel == null)
            {
                return;
            }

            for (int index = 0; index < _requiredExperienceByLevel.Length; index++)
            {
                _requiredExperienceByLevel[index] =
                    Mathf.Max(1, _requiredExperienceByLevel[index]);
            }
        }
    }
}
