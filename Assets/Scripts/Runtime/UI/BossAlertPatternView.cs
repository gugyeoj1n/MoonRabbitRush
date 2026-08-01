using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace MoonRabbitRush.UI
{
    [RequireComponent(typeof(RawImage))]
    public sealed class BossAlertPatternView : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _scrollSpeed = 1.2f;
        [SerializeField, Range(0f, 0.49f)]
        private float _verticalCrop = 0.2f;

        private RawImage _rawImage;
        private float _uvOffset;
        private CancellationTokenSource _scrollCts;

        private void Awake()
        {
            _rawImage = GetComponent<RawImage>();
            RefreshUvRect();
        }

        private void OnEnable()
        {
            RestartScrollLoop();
        }

        private void OnDisable()
        {
            CancelScrollLoop();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (_rawImage != null)
            {
                RefreshUvRect();
            }
        }

        private void RefreshUvRect()
        {
            RectTransform rectTransform = _rawImage.rectTransform;
            float height = Mathf.Max(1f, rectTransform.rect.height);
            float repeatCount = rectTransform.rect.width / height;
            float visibleHeight = 1f - _verticalCrop * 2f;

            _rawImage.uvRect = new Rect(
                _uvOffset,
                _verticalCrop,
                repeatCount,
                visibleHeight);
        }

        private void OnValidate()
        {
            _scrollSpeed = Mathf.Max(0f, _scrollSpeed);
            _verticalCrop = Mathf.Clamp(_verticalCrop, 0f, 0.49f);
        }

        private void RestartScrollLoop()
        {
            CancelScrollLoop();
            _scrollCts = CancellationTokenSource.CreateLinkedTokenSource(
                destroyCancellationToken);
            ScrollLoopAsync(_scrollCts.Token).Forget();
        }

        private void CancelScrollLoop()
        {
            if (_scrollCts == null)
            {
                return;
            }

            _scrollCts.Cancel();
            _scrollCts.Dispose();
            _scrollCts = null;
        }

        private async UniTaskVoid ScrollLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _uvOffset = Mathf.Repeat(
                    _uvOffset + _scrollSpeed * Time.unscaledDeltaTime,
                    1f);
                RefreshUvRect();
                await UniTask.Yield(
                    PlayerLoopTiming.Update,
                    cancellationToken);
            }
        }
    }
}
