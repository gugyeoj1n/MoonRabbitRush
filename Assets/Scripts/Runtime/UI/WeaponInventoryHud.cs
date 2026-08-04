using System.Collections.Generic;
using MoonRabbitRush.Weapons;
using UnityEngine;

namespace MoonRabbitRush
{
    public sealed class WeaponInventoryHud : MonoBehaviour
    {
        [SerializeField] private Transform _activeWeaponRect;
        [SerializeField] private Transform _passiveWeaponRect;
        [SerializeField] private CurrentWeaponIconView _iconPrefab;

        private readonly Dictionary<WeaponData, CurrentWeaponIconView> _views =
            new();
        private WeaponController _weaponController;

        private void Start()
        {
            _weaponController = FindAnyObjectByType<WeaponController>();
            if (_weaponController == null)
            {
                Debug.LogError("Weapon controller was not found for HUD.", this);
                return;
            }

            _weaponController.WeaponEquipped += HandleWeaponChanged;
            _weaponController.WeaponLeveledUp += HandleWeaponChanged;

            foreach (KeyValuePair<WeaponData, WeaponBehaviour> equipped in
                     _weaponController.EquippedWeapons)
            {
                HandleWeaponChanged(equipped.Key, equipped.Value.Level);
            }
        }

        private void OnDestroy()
        {
            if (_weaponController == null)
            {
                return;
            }

            _weaponController.WeaponEquipped -= HandleWeaponChanged;
            _weaponController.WeaponLeveledUp -= HandleWeaponChanged;
        }

        private void HandleWeaponChanged(WeaponData weapon, int level)
        {
            if (weapon == null)
            {
                return;
            }

            if (!_views.TryGetValue(weapon, out CurrentWeaponIconView view))
            {
                Transform parent = weapon.Category == WeaponCategory.Passive
                    ? _passiveWeaponRect
                    : _activeWeaponRect;

                if (_iconPrefab == null || parent == null)
                {
                    Debug.LogError("Weapon HUD references are missing.", this);
                    return;
                }

                view = Instantiate(_iconPrefab, parent);
                view.name = $"Current Weapon Icon - {weapon.DisplayName}";
                _views.Add(weapon, view);
            }

            view.Bind(weapon, level);
        }
    }
}
