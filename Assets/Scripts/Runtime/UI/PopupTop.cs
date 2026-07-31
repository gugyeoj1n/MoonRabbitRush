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

        // Register와 UnRegister는 UI말고 수정하는 곳에서 하도록 개선
        private void Awake()
        {
            DataBindingManager.Register(Property.PlayerHealth, 100);
            DataBindingManager.Register(Property.PlayerMaxHealth, 100);
        }

        private void Start()
        {
            DataBindingManager.BindText(Property.PlayerHealth, curHP);
            DataBindingManager.BindText(Property.PlayerMaxHealth, totalHP);
            DataBindingManager.BindSliderRatio(Property.PlayerHealth, Property.PlayerMaxHealth, HpSlider);

            DataBindingManager.BindText(Property.PlayerExperience, curHP);
            DataBindingManager.BindText(Property.PlayerExperience, totalHP);
            DataBindingManager.BindSliderRatio(Property.PlayerExperience, Property.PlayerMaxExperience, ExpSlider);

            DataBindingManager.BindText(Property.MonsterRemain, monsterNumber);
            DataBindingManager.BindText(Property.Wave, waveNumber);
        }

        private void OnDestroy()
        {
            DataBindingManager.UnRegister(Property.PlayerHealth);
            DataBindingManager.UnRegister(Property.PlayerMaxHealth);
        }

    }
}
