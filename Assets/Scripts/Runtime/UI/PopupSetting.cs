using R3;
using System;
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
        [SerializeField]
        private Toggle _cameraShakeToggle;
        [SerializeField]
        private GameObject _cameraShakeOn;
        [SerializeField]
        private GameObject _cameraShakeOff;

        private SoundManager _soundManager => ManagerRoot.Instance.SoundManager;

        protected override void Awake()
        {
            base.Awake();
            DataBindingManager.Register(Property.CameraShakeEnabled, true);
            DataBindingManager.BindToggle(Property.CameraShakeEnabled, _cameraShakeToggle);
        }

        private void OnEnable()
        {
            _sfxSlider.value = _soundManager.SfxVolume;
            _bgmSlider.value = _soundManager.BgmVolume;
            if (DataBindingManager.TryGetValue(Property.CameraShakeEnabled, out bool ison))
                _cameraShakeToggle.isOn = ison;

            _cameraShakeOn.SetActive(ison);
            _cameraShakeOff.SetActive(!ison);
            _sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            _bgmSlider.onValueChanged.AddListener(OnBgmVolumeChanged);
            _cameraShakeToggle.onValueChanged.AddListener(OnCameraShakeToggleChanged);
        }

        private void OnDisable()
        {
            _sfxSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
            _bgmSlider.onValueChanged.RemoveListener(OnBgmVolumeChanged);
            _cameraShakeToggle.onValueChanged.RemoveListener(OnCameraShakeToggleChanged);
        }

        private void OnSfxVolumeChanged(float value)
        {
            _soundManager.SfxVolume = value;
        }

        private void OnBgmVolumeChanged(float value)
        {
            _soundManager.BgmVolume = value;
        }

        private void OnCameraShakeToggleChanged(bool isOn)
        {
            _cameraShakeOn.SetActive(isOn);
            _cameraShakeOff.SetActive(!isOn);
        }
    }
}
