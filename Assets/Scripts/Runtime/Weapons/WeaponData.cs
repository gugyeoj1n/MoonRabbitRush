using UnityEngine;

namespace MoonRabbitRush.Weapons
{
    public enum WeaponCategory
    {
        Active,
        Passive
    }

    [CreateAssetMenu(
        fileName = "SO_Weapon_",
        menuName = "Moon Rabbit Rush/Weapons/Weapon Data")]
    public sealed class WeaponData : ScriptableObject
    {
        [Header("Display")]
        [SerializeField] private string _displayName;
        [SerializeField, TextArea] private string _description;
        [SerializeField] private Sprite _icon;
        [SerializeField] private WeaponCategory _category;

        [Header("Runtime")]
        [SerializeField] private WeaponBehaviour _behaviourPrefab;
        [SerializeField] private WeaponLevelStats[] _levels;
        [SerializeField] private PassiveWeaponLevelStats[] _passiveLevels;

        [Header("Active Skill")]
        [SerializeField, Min(0.1f)] private float _activeCooldown = 10f;

        public string DisplayName => _displayName;
        public string Description => _description;
        public Sprite Icon => _icon;
        public WeaponCategory Category => _category;
        public WeaponBehaviour BehaviourPrefab => _behaviourPrefab;
        public float ActiveCooldown => Mathf.Max(0.1f, _activeCooldown);
        public int MaxLevel => _category == WeaponCategory.Passive
            ? _passiveLevels?.Length ?? 0
            : _levels?.Length ?? 0;

        public bool IsValidLevel(int level)
        {
            return level >= 1 && level <= MaxLevel;
        }

        public bool TryGetLevelStats(int level, out WeaponLevelStats stats)
        {
            int index = level - 1;

            if (_levels == null || index < 0 || index >= _levels.Length)
            {
                stats = default;
                return false;
            }

            stats = _levels[index];
            return true;
        }

        public bool TryGetPassiveLevelStats(
            int level,
            out PassiveWeaponLevelStats stats)
        {
            int index = level - 1;
            if (_passiveLevels == null || index < 0 ||
                index >= _passiveLevels.Length)
            {
                stats = default;
                return false;
            }

            stats = _passiveLevels[index];
            return true;
        }
    }
}
