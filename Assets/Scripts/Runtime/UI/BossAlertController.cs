using Cysharp.Threading.Tasks;
using MoonRabbitRush.Waves;
using UnityEngine;

namespace MoonRabbitRush.UI
{
    public sealed class BossAlertController : MonoBehaviour
    {
        private static readonly float[] TextScaleSequence =
        {
            1f,
            1.1f,
            0.9f,
            1.1f,
            0.9f
        };

        [SerializeField] private WaveDirector _waveDirector;
        [SerializeField] private GameObject _alertRoot;
        [SerializeField] private RectTransform _alertText;
        [SerializeField, Min(0.1f)] private float _displayDuration = 3f;
        [SerializeField, Min(0.01f)] private float _scaleStepDuration = 0.16f;

        private bool _isPlaying;

        private void Awake()
        {
            if (_alertRoot != null)
            {
                _alertRoot.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (_waveDirector != null)
            {
                _waveDirector.AllConfiguredWavesCompleted += HandleWavesCompleted;
            }
        }

        private void OnDisable()
        {
            if (_waveDirector != null)
            {
                _waveDirector.AllConfiguredWavesCompleted -= HandleWavesCompleted;
            }
        }

        private void HandleWavesCompleted()
        {
            if (_isPlaying || _alertRoot == null || _alertText == null)
            {
                return;
            }

            PlayAlertAsync()
                .SuppressCancellationThrow()
                .Forget();
        }

        private async UniTask PlayAlertAsync()
        {
            _isPlaying = true;
            _alertRoot.SetActive(true);
            _alertText.localScale = Vector3.one;

            float totalElapsed = 0f;
            int sequenceIndex = 1;

            while (totalElapsed < _displayDuration)
            {
                float startScale = _alertText.localScale.x;
                float targetScale = TextScaleSequence[sequenceIndex];
                float stepElapsed = 0f;

                while (stepElapsed < _scaleStepDuration &&
                       totalElapsed < _displayDuration)
                {
                    await UniTask.Yield(
                        PlayerLoopTiming.Update,
                        destroyCancellationToken);

                    float deltaTime = Time.unscaledDeltaTime;
                    stepElapsed += deltaTime;
                    totalElapsed += deltaTime;
                    float progress = Mathf.Clamp01(
                        stepElapsed / _scaleStepDuration);
                    float scale = Mathf.Lerp(
                        startScale,
                        targetScale,
                        progress);
                    _alertText.localScale = Vector3.one * scale;
                }

                sequenceIndex =
                    (sequenceIndex + 1) % TextScaleSequence.Length;
            }

            _alertText.localScale = Vector3.one;
            _alertRoot.SetActive(false);
            _isPlaying = false;
        }

        private void OnValidate()
        {
            _displayDuration = Mathf.Max(0.1f, _displayDuration);
            _scaleStepDuration = Mathf.Max(0.01f, _scaleStepDuration);
        }
    }
}
