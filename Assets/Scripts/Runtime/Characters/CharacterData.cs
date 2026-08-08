using MoonRabbitRush.Player;
using MoonRabbitRush.Weapons;
using UnityEngine;

namespace MoonRabbitRush.Characters
{
    [CreateAssetMenu(
        fileName = "SO_Character_",
        menuName = "Moon Rabbit Rush/Characters/Character Data")]
    public sealed class CharacterData : ScriptableObject
    {
        [Header("Display")]
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _portrait;

        [Header("Animation")]
        [SerializeField] private Sprite[] _idleFrames;
        [SerializeField] private Sprite[] _moveFrames;
        [SerializeField] private Sprite[] _deathFrames;

        [Header("Gameplay")]
        [SerializeField] private PlayerStatsData _stats;
        [SerializeField] private WeaponData _startingWeapon;

        public string DisplayName => _displayName;
        public Sprite Portrait => _portrait;
        public Sprite[] IdleFrames => _idleFrames;
        public Sprite[] MoveFrames => _moveFrames;
        public Sprite[] DeathFrames => _deathFrames;
        public PlayerStatsData Stats => _stats;
        public WeaponData StartingWeapon => _startingWeapon;

        public bool IsValid =>
            !string.IsNullOrWhiteSpace(_displayName)
            && _stats != null
            && _startingWeapon != null
            && HasFrames(_idleFrames)
            && HasFrames(_moveFrames)
            && HasFrames(_deathFrames);

        private static bool HasFrames(Sprite[] frames)
        {
            return frames != null && frames.Length > 0;
        }
    }
}
