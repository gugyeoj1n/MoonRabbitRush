using System;
using System.Collections.Generic;
using MoonRabbitRush.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MoonRabbitRush.Weapons.Active
{
    public sealed class WeaponActiveSkillController : MonoBehaviour
    {
        private static readonly Key[] SlotKeys = { Key.Q, Key.W, Key.E };
        private static readonly string[] SlotLabels = { "Q", "W", "E" };

        [SerializeField] private Transform _playerRoot;
        [SerializeField] private GameStateManager _gameStateManager;

        private readonly List<WeaponActiveSlot> _slots = new();
        private WeaponController _weaponController;

        public IReadOnlyList<WeaponActiveSlot> Slots => _slots;
        public event Action<WeaponActiveSlot> SlotAdded;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();

            if (_weaponController != null)
            {
                _weaponController.WeaponEquipped += HandleWeaponChanged;
                _weaponController.WeaponLeveledUp += HandleWeaponChanged;
            }
        }

        private void OnDisable()
        {
            if (_weaponController != null)
            {
                _weaponController.WeaponEquipped -= HandleWeaponChanged;
                _weaponController.WeaponLeveledUp -= HandleWeaponChanged;
            }
        }

        private void Update()
        {
            foreach (WeaponActiveSlot slot in _slots)
            {
                slot.Tick(Time.deltaTime);
            }

            if (_gameStateManager != null && !_gameStateManager.IsPlaying)
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;

            if (keyboard == null)
            {
                return;
            }

            foreach (WeaponActiveSlot slot in _slots)
            {
                if (keyboard[slot.Key].wasPressedThisFrame)
                {
                    slot.TryActivate();
                }
            }
        }

        private void HandleWeaponChanged(WeaponData data, int level)
        {
            if (data == null || level < data.MaxLevel || _slots.Count >= SlotKeys.Length)
            {
                return;
            }

            foreach (WeaponActiveSlot slot in _slots)
            {
                if (slot.Data == data)
                {
                    return;
                }
            }

            if (!_weaponController.TryGetBehaviour(
                    data,
                    out WeaponBehaviour behaviour))
            {
                return;
            }

            int index = _slots.Count;
            var newSlot = new WeaponActiveSlot(
                behaviour,
                SlotKeys[index],
                SlotLabels[index]);
            _slots.Add(newSlot);
            SlotAdded?.Invoke(newSlot);
        }

        private void ResolveReferences()
        {
            if (_weaponController == null && _playerRoot != null)
            {
                _weaponController = _playerRoot.GetComponent<WeaponController>();
            }
        }
    }
}
