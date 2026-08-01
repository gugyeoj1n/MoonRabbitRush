using UnityEngine.InputSystem;

namespace MoonRabbitRush.Weapons.Active
{
    public sealed class WeaponActiveSlot
    {
        private readonly WeaponBehaviour _behaviour;
        private float _cooldownReadyTime;

        public WeaponActiveSlot(
            WeaponBehaviour behaviour,
            Key key,
            string keyLabel)
        {
            _behaviour = behaviour;
            Key = key;
            KeyLabel = keyLabel;
        }

        public WeaponData Data => _behaviour.Data;
        public Key Key { get; }
        public string KeyLabel { get; }
        public float CooldownRemaining =>
            UnityEngine.Mathf.Max(0f, _cooldownReadyTime - UnityEngine.Time.time);
        public bool IsCoolingDown => CooldownRemaining > 0f;

        public bool TryActivate()
        {
            if (IsCoolingDown || !_behaviour.TryActivateActiveSkill())
            {
                return false;
            }

            _cooldownReadyTime = UnityEngine.Time.time + Data.ActiveCooldown;
            return true;
        }
    }
}
