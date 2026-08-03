using System;
using MoonRabbitRush.Characters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MoonRabbitRush.UI
{
    [RequireComponent(typeof(Button))]
    public sealed class CharacterSelectionCardView : MonoBehaviour
    {
        [SerializeField] private Image _characterFace;
        [SerializeField] private Image _weaponIcon;
        [SerializeField] private TMP_Text _characterNameText;
        [SerializeField] private GameObject _selected;

        private Button _button;
        private Action<CharacterSelectionCardView> _clicked;

        public CharacterData Character { get; private set; }

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(HandleClicked);
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(HandleClicked);
            }
        }

        public void Bind(
            CharacterData character,
            Action<CharacterSelectionCardView> clicked)
        {
            Character = character;
            _clicked = clicked;

            if (_characterFace != null)
            {
                _characterFace.sprite = character.Portrait;
                _characterFace.enabled = character.Portrait != null;
            }

            if (_weaponIcon != null)
            {
                _weaponIcon.sprite = character.StartingWeapon.Icon;
                _weaponIcon.enabled = character.StartingWeapon.Icon != null;
            }

            if (_characterNameText != null)
            {
                _characterNameText.text = character.DisplayName;
            }

            SetSelected(false);
        }

        public void SetSelected(bool isSelected)
        {
            if (_selected != null)
            {
                _selected.SetActive(isSelected);
            }
        }

        private void HandleClicked()
        {
            _clicked?.Invoke(this);
        }
    }
}
