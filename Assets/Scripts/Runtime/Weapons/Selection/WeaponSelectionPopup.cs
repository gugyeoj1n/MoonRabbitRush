using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoonRabbitRush.Weapons.Selection
{
    public sealed class WeaponSelectionPopup : UIPopup
    {
        [SerializeField] private Transform _optionContainer;
        [SerializeField] private WeaponSelectionOptionView _optionPrefab;
        [SerializeField, Min(0f)] private float _optionEntranceInterval = 0.08f;

        private readonly List<WeaponSelectionOptionView> _optionViews = new();
        private Action<WeaponSelectionOption> _selected;

        public void Show(
            IReadOnlyList<WeaponSelectionOption> options,
            Action<WeaponSelectionOption> selected)
        {
            if (_optionContainer == null || _optionPrefab == null)
            {
                Debug.LogError("Weapon selection popup references are missing.", this);
                return;
            }

            ClearOptions();
            _selected = selected;
            gameObject.SetActive(true);

            for (int index = 0; index < options.Count; index++)
            {
                WeaponSelectionOptionView view = Instantiate(
                    _optionPrefab,
                    _optionContainer);
                view.Bind(
                    options[index],
                    HandleSelected,
                    index * _optionEntranceInterval);
                _optionViews.Add(view);
            }
        }

        public void Hide()
        {
            _selected = null;
            ClearOptions();
            gameObject.SetActive(false);
        }

        private void HandleSelected(WeaponSelectionOption option)
        {
            foreach (WeaponSelectionOptionView view in _optionViews)
            {
                view.SetInteractable(false);
            }

            _selected?.Invoke(option);
        }

        private void ClearOptions()
        {
            foreach (WeaponSelectionOptionView view in _optionViews)
            {
                if (view != null)
                {
                    Destroy(view.gameObject);
                }
            }

            _optionViews.Clear();
        }
    }
}
