using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MoonRabbitRush
{
    public class PopupTop : UIPopup
    {
        [SerializeField]
        private TextMeshProUGUI curHP;
        [SerializeField]
        private TextMeshProUGUI totalHP;
        [SerializeField]
        private TextMeshProUGUI monsterNumber;
        [SerializeField]
        private TextMeshProUGUI waveNumber;
        [SerializeField]
        private Slider HpSlider;
        [SerializeField]
        private Slider ExpSlider;

        private TextMeshProUGUI _expLabel;
        private GameObject _expDivider;
        private GameObject _expMaxLabel;
        private IDisposable _playerExperienceSubscription;
        private IDisposable _playerMaxExperienceSubscription;
        private IDisposable _playerLevelSubscription;
        private int _currentExperience;
        private int _requiredExperience;
        private int _currentLevel = 1;

        // Register와 UnRegister는 UI말고 수정하는 곳에서 하도록 개선
        private void Awake()
        {
            DataBindingManager.Register(Property.PlayerHealth, 100);
            DataBindingManager.Register(Property.PlayerMaxHealth, 100);
            CacheExperienceWidgets();
        }

        private void Start()
        {
            DataBindingManager.BindText(Property.PlayerHealth, curHP);
            DataBindingManager.BindText(Property.PlayerMaxHealth, totalHP);
            DataBindingManager.BindSliderRatio(Property.PlayerHealth, Property.PlayerMaxHealth, HpSlider);

            DataBindingManager.BindSliderRatio(Property.PlayerExperience, Property.PlayerMaxExperience, ExpSlider);
            SubscribeExperienceBindings();
            RefreshExperienceLabel();

            DataBindingManager.BindText(Property.MonsterRemain, monsterNumber);
            DataBindingManager.BindText(Property.Wave, waveNumber);
        }

        private void OnDestroy()
        {
            DataBindingManager.UnRegister(Property.PlayerHealth);
            DataBindingManager.UnRegister(Property.PlayerMaxHealth);
            _playerExperienceSubscription?.Dispose();
            _playerMaxExperienceSubscription?.Dispose();
            _playerLevelSubscription?.Dispose();
        }

        private void CacheExperienceWidgets()
        {
            if (ExpSlider == null)
            {
                return;
            }

            Transform expTextRoot = ExpSlider.transform.Find("Exp_Text");
            if (expTextRoot == null)
            {
                return;
            }

            _expLabel ??=
                expTextRoot.Find("CurrentHealth")?.GetComponent<TextMeshProUGUI>();
            _expDivider ??= expTextRoot.Find("Divider")?.gameObject;
            _expMaxLabel ??= expTextRoot.Find("MaxHealth")?.gameObject;
        }

        private void SubscribeExperienceBindings()
        {
            _playerExperienceSubscription?.Dispose();
            _playerMaxExperienceSubscription?.Dispose();
            _playerLevelSubscription?.Dispose();

            _playerExperienceSubscription =
                DataBindingManager.Subscribe(Property.PlayerExperience, value =>
                {
                    _currentExperience = value;
                    RefreshExperienceLabel();
                });

            _playerMaxExperienceSubscription =
                DataBindingManager.Subscribe(Property.PlayerMaxExperience, value =>
                {
                    _requiredExperience = value;
                    RefreshExperienceLabel();
                });

            _playerLevelSubscription =
                DataBindingManager.Subscribe(Property.PlayerLevel, value =>
                {
                    _currentLevel = Mathf.Max(1, value);
                    RefreshExperienceLabel();
                });

            if (DataBindingManager.TryGetValue(
                    Property.PlayerExperience,
                    out int currentExperience))
            {
                _currentExperience = currentExperience;
            }

            if (DataBindingManager.TryGetValue(
                    Property.PlayerMaxExperience,
                    out int requiredExperience))
            {
                _requiredExperience = requiredExperience;
            }

            if (DataBindingManager.TryGetValue(
                    Property.PlayerLevel,
                    out int currentLevel))
            {
                _currentLevel = Mathf.Max(1, currentLevel);
            }
        }

        private void RefreshExperienceLabel()
        {
            CacheExperienceWidgets();
            if (_expLabel == null)
            {
                return;
            }

            if (_expDivider != null)
            {
                _expDivider.SetActive(false);
            }

            if (_expMaxLabel != null)
            {
                _expMaxLabel.SetActive(false);
            }

            int percentage = _requiredExperience <= 0
                ? 0
                : Mathf.RoundToInt(
                    Mathf.Clamp01((float)_currentExperience / _requiredExperience)
                    * 100f);
            _expLabel.SetText("Lv. {0} ({1}%)", _currentLevel, percentage);
        }
    }
}
