using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace MoonRabbitRush
{
    public class PopupSystemUI : UIPopup
    {
        [SerializeField]
        private Slider _sfxSlider;
        [SerializeField]
        private Slider _bgmSlider;
        [SerializeField]
        private Toggle _toggle;
        [SerializeField]
        private GameObject _cameraShakeOn;
        [SerializeField]
        private GameObject _cameraShakeOff;
        private CancellationTokenSource _tokenSource;
        private SoundManager _soundManager => ManagerRoot.Instance.SoundManager;

        protected override void Awake()
        {
            base.Awake();
            DataBindingManager.Register(Property.CameraShakeEnabled, true);
            DataBindingManager.BindToggle(Property.CameraShakeEnabled, _toggle);
        }

        private void OnEnable()
        {
            _sfxSlider.value = _soundManager.SfxVolume;
            _bgmSlider.value = _soundManager.BgmVolume;
            if (DataBindingManager.TryGetValue(Property.CameraShakeEnabled, out bool ison))
                _toggle.isOn = ison;

            _cameraShakeOn.SetActive(ison);
            _cameraShakeOff.SetActive(!ison);
            _sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            _bgmSlider.onValueChanged.AddListener(OnBgmVolumeChanged);
            _toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        private void OnDisable()
        {
            _sfxSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
            _bgmSlider.onValueChanged.RemoveListener(OnBgmVolumeChanged);
            _toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        }

        private void OnSfxVolumeChanged(float value)
        {
            _soundManager.SfxVolume = value;
        }

        private void OnBgmVolumeChanged(float value)
        {
            _soundManager.BgmVolume = value;
        }        

        private void OnToggleValueChanged(bool isOn)
        {
            _cameraShakeOn.SetActive(isOn);
            _cameraShakeOff.SetActive(!isOn);
        }

        public void OnClickResume()
        {
            OnClickClose();
        }

        public void OnClickHome()
        {
            if (_tokenSource != null && !_tokenSource.IsCancellationRequested)
                return;

            var cancel = new CancellationTokenSource();
            _tokenSource = cancel;

            UniTask.Void(async token =>
            {
                try
                {
                    await ManagerRoot.Instance.SceneManager.TransitionTo(0, token);
                }
                finally
                {
                    cancel.Dispose();
                    _tokenSource = null;
                }
            }, cancel.Token);
        }

        public override void OnClickClose()
        {
            base.OnClickClose();
            Time.timeScale = 1f;
        }
    }
}
