using System;
using UnityEngine;

namespace MoonRabbitRush.Combat
{
    public sealed class CircleTelegraphView : MonoBehaviour
    {
        private SpriteRenderer _outlineRenderer;
        private SpriteRenderer _fillRenderer;
        private Transform _fillTransform;
        private float _duration;
        private float _elapsed;
        private bool _isActive;
        private bool _isReleased;

        public event Action<CircleTelegraphView> Released;

        public static CircleTelegraphView GetFromPool(string objectName)
        {
            const PoolType poolType = PoolType.TelegraphCircle;
            if (!PoolingManager.IsRegistered(poolType))
            {
                PoolingManager.RegisterPool(
                    poolType,
                    () =>
                    {
                        var telegraphObject = new GameObject(objectName);
                        return telegraphObject
                            .AddComponent<CircleTelegraphView>()
                            .gameObject;
                    },
                    defaultCapacity: 10,
                    maxSize: 100);
            }

            PoolingManager.GetObject(poolType, out GameObject pooledObject);
            if (pooledObject == null ||
                !pooledObject.TryGetComponent(
                    out CircleTelegraphView telegraph))
            {
                return null;
            }

            pooledObject.name = objectName;
            return telegraph;
        }

        private void Update()
        {
            if (!_isActive)
            {
                return;
            }

            _elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(_elapsed / _duration);
            _fillTransform.localScale = new Vector3(progress, progress, 1f);

            if (_elapsed >= _duration)
            {
                Release();
            }
        }

        public void Initialize(
            Vector2 center,
            float radius,
            float duration,
            Sprite outlineSprite,
            Sprite fillSprite,
            Color outlineColor,
            Color fillColor,
            float verticalScale)
        {
            if (outlineSprite == null || fillSprite == null)
            {
                Debug.LogError("Telegraph outline and fill sprites are required.", this);
                Release();
                return;
            }

            CreateRenderersIfNeeded();
            transform.position = center;

            float diameter = Mathf.Max(0.01f, radius * 2f);
            float spriteWidth = Mathf.Max(0.01f, outlineSprite.bounds.size.x);
            float scale = diameter / spriteWidth;
            transform.localScale = new Vector3(
                scale,
                scale * Mathf.Clamp(verticalScale, 0.1f, 1f),
                1f);

            _outlineRenderer.sprite = outlineSprite;
            _outlineRenderer.color = outlineColor;
            _fillRenderer.sprite = fillSprite;
            _fillRenderer.color = fillColor;
            _fillTransform.localScale = new Vector3(0f, 0f, 1f);
            _duration = Mathf.Max(0.05f, duration);
            _elapsed = 0f;
            _isReleased = false;
            _isActive = true;
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

            PoolingManager.Release(PoolType.TelegraphCircle, gameObject);
        }

        private void CreateRenderersIfNeeded()
        {
            if (_outlineRenderer != null && _fillRenderer != null)
            {
                return;
            }

            var fillObject = new GameObject("Fill");
            fillObject.transform.SetParent(transform, false);
            _fillTransform = fillObject.transform;
            _fillRenderer = fillObject.AddComponent<SpriteRenderer>();
            _fillRenderer.sortingOrder = 4;

            var outlineObject = new GameObject("Outline");
            outlineObject.transform.SetParent(transform, false);
            _outlineRenderer = outlineObject.AddComponent<SpriteRenderer>();
            _outlineRenderer.sortingOrder = 5;
        }
    }
}
