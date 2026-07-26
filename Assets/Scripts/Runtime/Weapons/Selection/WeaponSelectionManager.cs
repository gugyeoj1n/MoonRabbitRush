using System.Collections;
using System.Collections.Generic;
using MoonRabbitRush.Core;
using MoonRabbitRush.Progression;
using UnityEngine;

namespace MoonRabbitRush.Weapons.Selection
{
    public sealed class WeaponSelectionManager : MonoBehaviour
    {
        private const int OptionCount = 3;

        [SerializeField] private PlayerExperience _playerExperience;
        [SerializeField] private WeaponController _weaponController;
        [SerializeField] private GameStateManager _gameStateManager;
        [SerializeField] private WeaponSelectionPopup _selectionPopup;
        [SerializeField] private WeaponData[] _weaponPool;

        private readonly List<WeaponSelectionOption> _candidates = new();
        private readonly List<WeaponSelectionOption> _visibleOptions = new();
        private int _pendingSelections;
        private bool _isSelecting;

        private void Awake()
        {
            ResolveReferences();
        }

        private void Start()
        {
            if (_selectionPopup != null)
            {
                _selectionPopup.Hide();
            }
        }

        private void OnEnable()
        {
            ResolveReferences();

            if (_playerExperience != null)
            {
                _playerExperience.LeveledUp += HandleLevelUp;
            }
        }

        private void OnDisable()
        {
            if (_playerExperience != null)
            {
                _playerExperience.LeveledUp -= HandleLevelUp;
            }
        }

        private void HandleLevelUp(int level)
        {
            _pendingSelections++;

            if (!_isSelecting)
            {
                OpenSelection();
            }
        }

        private void OpenSelection()
        {
            if (!ValidateReferences())
            {
                return;
            }

            BuildCandidates();
            if (_candidates.Count == 0)
            {
                Debug.LogWarning("No selectable weapon remains.", this);
                _pendingSelections = 0;
                _isSelecting = false;

                if (_gameStateManager.CurrentState == InGameState.LevelUp)
                {
                    _gameStateManager.TryChangeState(InGameState.Playing);
                }

                return;
            }

            if (_gameStateManager.CurrentState == InGameState.Playing &&
                !_gameStateManager.TryChangeState(InGameState.LevelUp))
            {
                return;
            }

            _isSelecting = true;
            SelectRandomOptions();
            _selectionPopup.Show(_visibleOptions, HandleOptionSelected);
        }

        private void HandleOptionSelected(WeaponSelectionOption option)
        {
            if (!_isSelecting || !_weaponController.Equip(option.Weapon))
            {
                return;
            }

            _selectionPopup.Hide();
            _pendingSelections = Mathf.Max(0, _pendingSelections - 1);
            _isSelecting = false;

            if (_pendingSelections > 0)
            {
                StartCoroutine(OpenNextSelection());
                return;
            }

            _gameStateManager.TryChangeState(InGameState.Playing);
        }

        private IEnumerator OpenNextSelection()
        {
            yield return null;
            OpenSelection();
        }

        private void BuildCandidates()
        {
            _candidates.Clear();

            if (_weaponPool == null)
            {
                return;
            }

            foreach (WeaponData weapon in _weaponPool)
            {
                if (weapon == null || weapon.MaxLevel <= 0)
                {
                    continue;
                }

                _weaponController.TryGetLevel(weapon, out int currentLevel);
                int targetLevel = currentLevel + 1;

                if (targetLevel <= weapon.MaxLevel)
                {
                    _candidates.Add(new WeaponSelectionOption(
                        weapon,
                        currentLevel,
                        targetLevel));
                }
            }
        }

        private void SelectRandomOptions()
        {
            _visibleOptions.Clear();

            for (int index = _candidates.Count - 1; index > 0; index--)
            {
                int swapIndex = Random.Range(0, index + 1);
                (_candidates[index], _candidates[swapIndex]) =
                    (_candidates[swapIndex], _candidates[index]);
            }

            int count = Mathf.Min(OptionCount, _candidates.Count);
            for (int index = 0; index < count; index++)
            {
                _visibleOptions.Add(_candidates[index]);
            }
        }

        private void ResolveReferences()
        {
            _playerExperience ??= FindAnyObjectByType<PlayerExperience>();
            _weaponController ??= FindAnyObjectByType<WeaponController>();
            _gameStateManager ??= FindAnyObjectByType<GameStateManager>();
            _selectionPopup ??= FindAnyObjectByType<WeaponSelectionPopup>(
                FindObjectsInactive.Include);
        }

        private bool ValidateReferences()
        {
            bool isValid =
                _playerExperience != null &&
                _weaponController != null &&
                _gameStateManager != null &&
                _selectionPopup != null;

            if (!isValid)
            {
                Debug.LogError(
                    "Weapon selection manager references are missing.",
                    this);
            }

            return isValid;
        }
    }
}
