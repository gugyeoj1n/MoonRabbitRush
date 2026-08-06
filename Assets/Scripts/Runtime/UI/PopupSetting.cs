using UnityEngine;
using UnityEngine.UI;

namespace MoonRabbitRush
{
    public class PopupSetting : UIPopup
    {
        [SerializeField]
        private Slider _sfxSlider;
        [SerializeField]
        private Slider _bgmSlider;

        private SoundManager _soundManager => ManagerRoot.Instance.SoundManager;

        private void OnEnable()
        {
            _sfxSlider.value = _soundManager.SfxVolume;
            _bgmSlider.value = _soundManager.BgmVolume;

            _sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            _bgmSlider.onValueChanged.AddListener(OnBgmVolumeChanged);
        }

        private void OnDisable()
        {
            _sfxSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
            _bgmSlider.onValueChanged.RemoveListener(OnBgmVolumeChanged);
        }

        private void OnSfxVolumeChanged(float value)
        {
            _soundManager.SfxVolume = value;
        }

        private void OnBgmVolumeChanged(float value)
        {
            _soundManager.BgmVolume = value;
        }
    }
}
