using UnityEngine.InputSystem;

namespace MoonRabbitRush.Weapons.Active
{
    public sealed class WeaponActiveSlot
    {
        private readonly WeaponBehaviour _behaviour;

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
        public float CooldownRemaining { get; private set; }
        public bool IsCoolingDown => CooldownRemaining > 0f;

        public void Tick(float deltaTime)
        {
            CooldownRemaining =
                UnityEngine.Mathf.Max(0f, CooldownRemaining - deltaTime);
        }

        public bool TryActivate()
        {
            if (IsCoolingDown || !_behaviour.TryActivateActiveSkill())
            {
                return false;
            }

            CooldownRemaining = Data.ActiveCooldown;
            return true;
        }
    }
}
