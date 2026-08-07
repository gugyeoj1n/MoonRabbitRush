using MoonRabbitRush.Core;
using MoonRabbitRush.Enemies;
using MoonRabbitRush.Enemies.Bosses;
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
        [SerializeField]
        private Slider BossHpSlider;
        [SerializeField]
        private TextMeshProUGUI BossCurrentHpText;
        [SerializeField]
        private TextMeshProUGUI BossMaxHpText;
        [SerializeField]
        private GameStateManager _gameStateManager;

        private TextMeshProUGUI _expLabel;
        private GameObject _expDivider;
        private GameObject _expMaxLabel;
        private BossEncounterController _bossEncounterController;
        private EnemyHealth _bossHealth;
        private IDisposable _playerExperienceSubscription;
        private IDisposable _playerMaxExperienceSubscription;
        private IDisposable _playerLevelSubscription;
        private int _currentExperience;
        private int _requiredExperience;
        private int _currentLevel = 1;

        // Register와 UnRegister는 UI말고 수정하는 곳에서 하도록 개선
        protected override void Awake()
        {
            base.Awake();
            DataBindingManager.Register(Property.PlayerHealth, 100);
            DataBindingManager.Register(Property.PlayerMaxHealth, 100);
            CacheExperienceWidgets();
            SetBossHpVisible(false);            
        }

        private void OnEnable()
        {
            ResolveBossEncounterController();
            SubscribeBossEvents();
            _gameStateManager.StateChanged += HandleStateChanged;
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

        private void OnDisable()
        {
            UnsubscribeBossEvents();
            UnbindBossHealth();
            _gameStateManager.StateChanged -= HandleStateChanged;
        }

        private void OnDestroy()
        {
            DataBindingManager.UnRegister(Property.PlayerHealth);
            DataBindingManager.UnRegister(Property.PlayerMaxHealth);
            _playerExperienceSubscription?.Dispose();
            _playerMaxExperienceSubscription?.Dispose();
            _playerLevelSubscription?.Dispose();
        }

        private void ResolveBossEncounterController()
        {
            _bossEncounterController ??= FindAnyObjectByType<BossEncounterController>();
        }

        private void SubscribeBossEvents()
        {
            if (_bossEncounterController == null)
            {
                return;
            }

            _bossEncounterController.BossSpawned -= HandleBossSpawned;
            _bossEncounterController.BossSpawned += HandleBossSpawned;
            _bossEncounterController.BossDefeated -= HandleBossDefeated;
            _bossEncounterController.BossDefeated += HandleBossDefeated;
        }

        private void UnsubscribeBossEvents()
        {
            if (_bossEncounterController == null)
            {
                return;
            }

            _bossEncounterController.BossSpawned -= HandleBossSpawned;
            _bossEncounterController.BossDefeated -= HandleBossDefeated;
        }

        private void HandleBossSpawned(EnemyActor boss)
        {
            BindBossHealth(boss != null ? boss.Health : null);
        }

        private void HandleBossDefeated()
        {
            UnbindBossHealth();
            SetBossHpVisible(false);
        }

        private void BindBossHealth(EnemyHealth bossHealth)
        {
            UnbindBossHealth();
            _bossHealth = bossHealth;

            if (_bossHealth == null || BossHpSlider == null)
            {
                SetBossHpVisible(false);
                return;
            }

            _bossHealth.HealthChanged += HandleBossHealthChanged;
            SetBossHpVisible(true);
            HandleBossHealthChanged(_bossHealth.CurrentHealth, _bossHealth.MaxHealth);
        }

        private void UnbindBossHealth()
        {
            if (_bossHealth == null)
            {
                return;
            }

            _bossHealth.HealthChanged -= HandleBossHealthChanged;
            _bossHealth = null;
        }

        private void HandleBossHealthChanged(float current, float max)
        {
            if (BossHpSlider == null)
            {
                return;
            }

            BossHpSlider.value = max <= 0f ? 0f : current / max;

            if (BossCurrentHpText != null)
            {
                BossCurrentHpText.SetText("{0}", Mathf.CeilToInt(current));
            }

            if (BossMaxHpText != null)
            {
                BossMaxHpText.SetText("{0}", Mathf.CeilToInt(max));
            }
        }

        private void SetBossHpVisible(bool visible)
        {
            if (BossHpSlider != null)
            {
                BossHpSlider.gameObject.SetActive(visible);
            }

            if (BossMaxHpText != null)
            {
                BossMaxHpText.gameObject.SetActive(visible);
            }
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

        private void HandleStateChanged(InGameState previous, InGameState current)
        {
            if (current != InGameState.GameOver)
            {
                return;
            }

            gameObject.SetActive(false);
        }
    }
}
