using System;
using UnityEngine;

namespace MoonRabbitRush.Combat
{
    public sealed class CircleTelegraphView : MonoBehaviour
    {
        private const int TextureSize = 64;
        private const int RingThickness = 4;

        private static Sprite _ringSprite;

        private SpriteRenderer _renderer;
        private Color _startColor;
        private Color _endColor;
        private float _duration;
        private float _elapsed;
        private bool _isActive;
        private bool _isReleased;

        public event Action<CircleTelegraphView> Released;

        private void Awake()
        {
            _renderer = gameObject.AddComponent<SpriteRenderer>();
            _renderer.sprite = GetOrCreateRingSprite();
            _renderer.sortingOrder = 4;
        }

        private void Update()
        {
            if (!_isActive)
            {
                return;
            }

            _elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(_elapsed / _duration);
            _renderer.color = Color.Lerp(_startColor, _endColor, progress);

            float pulse = 1f + Mathf.Sin(progress * Mathf.PI * 8f) * 0.03f;
            transform.localScale = Vector3.one * (_radiusDiameter * pulse);

            if (_elapsed >= _duration)
            {
                Release();
            }
        }

        private float _radiusDiameter;

        public void Initialize(
            Vector2 center,
            float radius,
            float duration,
            Color startColor,
            Color endColor)
        {
            transform.position = center;
            _radiusDiameter = Mathf.Max(0.01f, radius * 2f);
            transform.localScale = Vector3.one * _radiusDiameter;
            _duration = Mathf.Max(0.05f, duration);
            _startColor = startColor;
            _endColor = endColor;
            _elapsed = 0f;
            _isReleased = false;
            _isActive = true;
            _renderer.color = _startColor;
        }

        public void Release()
        {
            if (_isReleased)
            {
                return;
            }

            _isReleased = true;
            _isActive = false;

            if (Released != null)
            {
                Released.Invoke(this);
                return;
            }

            Destroy(gameObject);
        }

        private static Sprite GetOrCreateRingSprite()
        {
            if (_ringSprite != null)
            {
                return _ringSprite;
            }

            var texture = new Texture2D(
                TextureSize,
                TextureSize,
                TextureFormat.RGBA32,
                false)
            {
                name = "Generated_CircleTelegraph",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };

            var pixels = new Color[TextureSize * TextureSize];
            Vector2 center = Vector2.one * (TextureSize - 1) * 0.5f;
            float outerRadius = TextureSize * 0.5f - 1f;
            float innerRadius = outerRadius - RingThickness;

            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    bool isRing = distance >= innerRadius && distance <= outerRadius;
                    pixels[y * TextureSize + x] = isRing ? Color.white : Color.clear;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            _ringSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, TextureSize, TextureSize),
                new Vector2(0.5f, 0.5f),
                TextureSize);
            _ringSprite.name = "Generated_CircleTelegraph";
            _ringSprite.hideFlags = HideFlags.HideAndDontSave;
            return _ringSprite;
        }
    }
}
