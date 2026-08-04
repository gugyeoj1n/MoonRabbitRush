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

        [Header("Active Skill")]
        [SerializeField, Min(0.1f)] private float _activeCooldown = 10f;

        public string DisplayName => _displayName;
        public string Description => _description;
        public Sprite Icon => _icon;
        public WeaponCategory Category => _category;
        public WeaponBehaviour BehaviourPrefab => _behaviourPrefab;
        public float ActiveCooldown => Mathf.Max(0.1f, _activeCooldown);
        public int MaxLevel => _levels?.Length ?? 0;

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
    }
}
