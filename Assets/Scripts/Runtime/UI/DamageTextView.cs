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
        private Vector2 _startPosition;
        private Color _startColor;
        private float _elapsed;

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
            _text = GetComponent<TMP_Text>();
            _text.outlineColor = _outlineColor;
            _text.outlineWidth = _outlineWidth;
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(_elapsed / _duration);
            float easedProgress = 1f - Mathf.Pow(1f - progress, 2f);

            _rectTransform.anchoredPosition =
                _startPosition + Vector2.up * (_riseDistance * easedProgress);

            Color color = _startColor;
            color.a = 1f - progress;
            _text.color = color;

            Color outlineColor = _outlineColor;
            outlineColor.a = 1f - progress;
            _text.outlineColor = outlineColor;

            if (progress >= 1f)
            {
                Destroy(gameObject);
            }
        }

        public void Initialize(float amount, Vector2 anchoredPosition)
        {
            _elapsed = 0f;
            _startPosition = anchoredPosition;
            _startColor = _text.color;
            _rectTransform.anchoredPosition = anchoredPosition;
            _text.SetText("{0:0.#}", amount);
        }
    }
}
