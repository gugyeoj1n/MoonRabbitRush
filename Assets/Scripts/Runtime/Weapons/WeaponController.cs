using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoonRabbitRush.Weapons
{
    public sealed class WeaponController : MonoBehaviour
    {
        [SerializeField] private WeaponData[] _startingWeapons;

        private readonly Dictionary<WeaponData, WeaponBehaviour> _equippedWeapons = new();

        public event Action<WeaponData, int> WeaponEquipped;
        public event Action<WeaponData, int> WeaponLeveledUp;

        public IEnumerable<KeyValuePair<WeaponData, WeaponBehaviour>>
            EquippedWeapons => _equippedWeapons;

        public void ConfigureStartingWeapon(WeaponData weapon)
        {
            _startingWeapons = weapon != null
                ? new[] { weapon }
                : Array.Empty<WeaponData>();
        }

        private void Start()
        {
            if (_startingWeapons == null)
            {
                return;
            }

            foreach (WeaponData weapon in _startingWeapons)
            {
                Equip(weapon);
            }
        }

        public bool Equip(WeaponData data)
        {
            if (data == null || data.BehaviourPrefab == null || data.MaxLevel == 0)
            {
                Debug.LogError("Weapon data is incomplete.", data);
                return false;
            }

            if (_equippedWeapons.ContainsKey(data))
            {
                return LevelUp(data);
            }

            WeaponBehaviour behaviour = Instantiate(
                data.BehaviourPrefab,
                transform.position,
                Quaternion.identity,
                transform);
            behaviour.Initialize(transform, data, 1);
            _equippedWeapons.Add(data, behaviour);
            WeaponEquipped?.Invoke(data, 1);
            return true;
        }

        public bool LevelUp(WeaponData data)
        {
            if (!_equippedWeapons.TryGetValue(data, out WeaponBehaviour behaviour))
            {
                return Equip(data);
            }

            int nextLevel = behaviour.Level + 1;

            if (!behaviour.SetLevel(nextLevel))
            {
                return false;
            }

            WeaponLeveledUp?.Invoke(data, nextLevel);
            return true;
        }

        public bool TryGetLevel(WeaponData data, out int level)
        {
            if (_equippedWeapons.TryGetValue(data, out WeaponBehaviour behaviour))
            {
                level = behaviour.Level;
                return true;
            }

            level = 0;
            return false;
        }

        public bool TryGetBehaviour(
            WeaponData data,
            out WeaponBehaviour behaviour)
        {
            return _equippedWeapons.TryGetValue(data, out behaviour);
        }
    }
}
