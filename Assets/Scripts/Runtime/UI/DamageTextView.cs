using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace MoonRabbitRush.UI
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(TMP_Text))]
    public sealed class DamageTextView : MonoBehaviour
    {
        [SerializeField, Min(0.05f)] private float _duration = 0.75f;
        [SerializeField, Min(0f)] private float _riseDistance = 45f;
        [SerializeField] private Color32 _outlineColor = new(55, 25, 10, 255);
        [SerializeField, Range(0f, 1f)] private float _outlineWidth = 0.18f;

        private RectTransform _rectTransform;
        private TMP_Text _text;
        private Camera _worldCamera;
        private Camera _uiCamera;
        private RectTransform _container;
        private Vector3 _worldPosition;
        private Vector2 _screenOffset;
        private Color _startColor;
        private float _elapsed;
        private CancellationTokenSource _animationCts;

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
            _text = GetComponent<TMP_Text>();
            _text.outlineColor = _outlineColor;
            _text.outlineWidth = _outlineWidth;
        }

        private void OnDisable()
        {
            CancelAnimation();
        }

        public void Initialize(
            float amount,
            Vector3 worldPosition,
            Vector2 screenOffset,
            Camera worldCamera,
            RectTransform container,
            Camera uiCamera,
            Color32? colorOverride = null)
        {
            _elapsed = 0f;
            _worldPosition = worldPosition;
            _screenOffset = screenOffset;
            _worldCamera = worldCamera;
            _container = container;
            _uiCamera = uiCamera;
            _startColor = colorOverride ?? _text.color;
            _text.color = _startColor;
            _text.SetText("{0:0}", amount);
            UpdateScreenPosition(0f);
            RestartAnimation();
        }

        private void UpdateScreenPosition(float riseProgress)
        {
            if (_worldCamera == null || _container == null)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 screenPosition = _worldCamera.WorldToScreenPoint(_worldPosition);
            if (screenPosition.z < 0f)
            {
                _text.enabled = false;
                return;
            }

            _text.enabled = true;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _container,
                    screenPosition,
                    _uiCamera,
                    out Vector2 localPosition))
            {
                _rectTransform.anchoredPosition =
                    localPosition
                    + _screenOffset
                    + Vector2.up * (_riseDistance * riseProgress);
            }
        }

        private void RestartAnimation()
        {
            CancelAnimation();
            _animationCts = CancellationTokenSource.CreateLinkedTokenSource(
                destroyCancellationToken);
            AnimateAsync(_animationCts.Token).Forget();
        }

        private void CancelAnimation()
        {
            if (_animationCts == null)
            {
                return;
            }

            _animationCts.Cancel();
            _animationCts.Dispose();
            _animationCts = null;
        }

        private async UniTaskVoid AnimateAsync(CancellationToken cancellationToken)
        {
            while (_elapsed < _duration && !cancellationToken.IsCancellationRequested)
            {
                _elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(_elapsed / _duration);
                float easedProgress = 1f - Mathf.Pow(1f - progress, 2f);

                UpdateScreenPosition(easedProgress);

                Color color = _startColor;
                color.a = 1f - progress;
                _text.color = color;

                Color outlineColor = _outlineColor;
                outlineColor.a = 1f - progress;
                _text.outlineColor = outlineColor;

                await UniTask.Yield(
                    PlayerLoopTiming.Update,
                    cancellationToken);
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                Destroy(gameObject);
            }
        }
    }
}
