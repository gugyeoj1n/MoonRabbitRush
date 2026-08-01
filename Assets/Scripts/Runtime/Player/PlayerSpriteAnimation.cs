using UnityEngine;

namespace MoonRabbitRush.Player
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(PlayerHealth))]
    public sealed class PlayerSpriteAnimation : MonoBehaviour
    {
        [SerializeField] private Sprite[] _idleFrames;
        [SerializeField] private Sprite[] _moveFrames;
        [SerializeField, Min(1f)] private float _idleFrameRate = 8f;
        [SerializeField, Min(1f)] private float _moveFrameRate = 10f;
        [SerializeField, Min(0f)] private float _moveThreshold = 0.01f;
        [SerializeField] private bool _flipWithMovement = true;

        private PlayerMovement _movement;
        private PlayerHealth _health;
        private SpriteRenderer _spriteRenderer;
        private Sprite[] _currentFrames;
        private float _currentFrameRate;
        private float _elapsed;
        private int _frameIndex;

        private void Awake()
        {
            _movement = GetComponent<PlayerMovement>();
            _health = GetComponent<PlayerHealth>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            SetSequence(_idleFrames, _idleFrameRate);
        }

        private void Update()
        {
            if (_health != null && !_health.IsAlive)
            {
                return;
            }

            Vector2 moveInput = _movement != null ? _movement.MoveInput : Vector2.zero;
            bool isMoving = moveInput.sqrMagnitude > _moveThreshold * _moveThreshold;

            if (_flipWithMovement && Mathf.Abs(moveInput.x) > _moveThreshold)
            {
                _spriteRenderer.flipX = moveInput.x < 0f;
            }

            SetSequence(
                isMoving ? _moveFrames : _idleFrames,
                isMoving ? _moveFrameRate : _idleFrameRate);

            if (_currentFrames == null || _currentFrames.Length <= 1)
            {
                return;
            }

            _elapsed += Time.deltaTime;
            float frameDuration = 1f / _currentFrameRate;

            while (_elapsed >= frameDuration)
            {
                _elapsed -= frameDuration;
                _frameIndex = (_frameIndex + 1) % _currentFrames.Length;
                _spriteRenderer.sprite = _currentFrames[_frameIndex];
            }
        }

        private void SetSequence(Sprite[] frames, float frameRate)
        {
            if (ReferenceEquals(_currentFrames, frames))
            {
                return;
            }

            _currentFrames = frames;
            _currentFrameRate = frameRate;
            _elapsed = 0f;
            _frameIndex = 0;

            if (_currentFrames != null && _currentFrames.Length > 0)
            {
                _spriteRenderer.sprite = _currentFrames[0];
            }
        }
    }
}
