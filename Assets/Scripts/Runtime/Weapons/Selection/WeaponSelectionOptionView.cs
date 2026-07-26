using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MoonRabbitRush.Weapons.Selection
{
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class WeaponSelectionOptionView : MonoBehaviour
    {
        [SerializeField] private Image _weaponIcon;
        [SerializeField] private TMP_Text _weaponNameText;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField, Min(0.05f)] private float _entranceDuration = 0.2f;
        [SerializeField, Range(0.5f, 1f)] private float _entranceScale = 0.85f;

        private Button _button;
        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        private WeaponSelectionOption _option;
        private Action<WeaponSelectionOption> _selected;
        private bool _canSelect;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _rectTransform = (RectTransform)transform;
            _button.onClick.AddListener(HandleClick);
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(HandleClick);
            }
        }

        public void Bind(
            in WeaponSelectionOption option,
            Action<WeaponSelectionOption> selected,
            float entranceDelay)
        {
            _option = option;
            _selected = selected;
            _canSelect = true;

            _weaponIcon.sprite = option.Weapon.Icon;
            _weaponIcon.enabled = option.Weapon.Icon != null;
            _weaponNameText.SetText(option.Weapon.DisplayName);
            _levelText.SetText(
                option.IsNew
                    ? $"Lv.{option.TargetLevel}"
                    : $"Lv.{option.CurrentLevel} >> Lv.{option.TargetLevel}");
            _descriptionText.SetText(option.Weapon.Description);

            StopAllCoroutines();
            StartCoroutine(PlayEntrance(entranceDelay));
        }

        public void SetInteractable(bool interactable)
        {
            _canSelect = interactable;
            _button.interactable = interactable;
        }

        private IEnumerator PlayEntrance(float delay)
        {
            _canvasGroup.alpha = 0f;
            _rectTransform.localScale = Vector3.one * _entranceScale;

            float delayElapsed = 0f;
            while (delayElapsed < delay)
            {
                delayElapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            float elapsed = 0f;
            while (elapsed < _entranceDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / _entranceDuration);
                float eased = 1f - Mathf.Pow(1f - progress, 3f);
                _canvasGroup.alpha = eased;
                _rectTransform.localScale =
                    Vector3.one * Mathf.Lerp(_entranceScale, 1f, eased);
                yield return null;
            }

            _canvasGroup.alpha = 1f;
            _rectTransform.localScale = Vector3.one;
        }

        private void HandleClick()
        {
            if (!_canSelect)
            {
                return;
            }

            _canSelect = false;
            _selected?.Invoke(_option);
        }
    }
}
