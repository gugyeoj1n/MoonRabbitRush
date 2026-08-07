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
        private CancellationTokenSource _tokenSource;
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
