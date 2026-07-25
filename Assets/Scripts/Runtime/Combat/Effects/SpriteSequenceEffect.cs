using System;
using UnityEngine;

namespace MoonRabbitRush.Combat
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class SpriteSequenceEffect : MonoBehaviour
    {
        [SerializeField] private Sprite[] _frames;
        [SerializeField, Min(1f)] private float _framesPerSecond = 16f;

        private SpriteRenderer _spriteRenderer;
        private float _elapsed;
        private int _frameIndex;
        private bool _isReleased;

        public event Action<SpriteSequenceEffect> Released;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            _elapsed = 0f;
            _frameIndex = 0;
            _isReleased = false;

            if (_frames == null || _frames.Length == 0)
            {
                Debug.LogError("Sprite sequence frames are not assigned.", this);
                Release();
                return;
            }

            _spriteRenderer.sprite = _frames[0];
        }

        private void Update()
        {
            if (_isReleased)
            {
                return;
            }

            _elapsed += Time.deltaTime;
            int nextFrame = Mathf.FloorToInt(_elapsed * _framesPerSecond);

            if (nextFrame >= _frames.Length)
            {
                Release();
                return;
            }

            if (nextFrame != _frameIndex)
            {
                _frameIndex = nextFrame;
                _spriteRenderer.sprite = _frames[_frameIndex];
            }
        }

        public void Release()
        {
            if (_isReleased)
            {
                return;
            }

            _isReleased = true;

            if (Released != null)
            {
                Released.Invoke(this);
                return;
            }

            Destroy(gameObject);
        }
    }
}
