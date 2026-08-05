using UnityEngine;

namespace MoonRabbitRush.Player
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PlayerSpriteAnimation : MonoBehaviour
    {
        [SerializeField] private Sprite[] _idleFrames;
        [SerializeField] private Sprite[] _moveFrames;
        [SerializeField] private Sprite[] _deathFrames;
        [SerializeField, Min(1f)] private float _idleFrameRate = 8f;
        [SerializeField, Min(1f)] private float _moveFrameRate = 10f;
        [SerializeField, Min(1f)] private float _deathFrameRate = 8f;
        [SerializeField, Min(0f)] private float _moveThreshold = 0.01f;
        [SerializeField] private bool _flipWithMovement = true;

        private PlayerMovement _movement;
        private PlayerHealth _health;
        private SpriteRenderer _spriteRenderer;
        private Sprite[] _currentFrames;
        private float _currentFrameRate;
        private float _elapsed;
        private int _frameIndex;
        private bool _isPlayingDeath;
        private Vector3 _defaultLocalPosition;
        private float _deathGroundY;

        public float DeathAnimationDuration
        {
            get
            {
                int transitionCount = Mathf.Max(0, (_deathFrames?.Length ?? 0) - 1);
                return transitionCount / Mathf.Max(1f, _deathFrameRate);
            }
        }

        private void Awake()
        {
            _movement = GetComponentInParent<PlayerMovement>();
            _health = GetComponentInParent<PlayerHealth>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _defaultLocalPosition = transform.localPosition;
        }

        private void OnEnable()
        {
            if (_health != null)
            {
                _health.Died += PlayDeath;
            }

            transform.localPosition = _defaultLocalPosition;
            _isPlayingDeath = false;
            SetSequence(_idleFrames, _idleFrameRate);
        }

        private void OnDisable()
        {
            if (_health != null)
            {
                _health.Died -= PlayDeath;
            }
        }

        private void Update()
        {
            if (_isPlayingDeath)
            {
                UpdateSequence(loop: false);
                return;
            }

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

            UpdateSequence(loop: true);
        }

        private void PlayDeath()
        {
            _deathGroundY = GetSpriteBottomY();
            _isPlayingDeath = true;
            SetSequence(_deathFrames, _deathFrameRate);
            AlignCurrentFrameToDeathGround();
        }

        private void UpdateSequence(bool loop)
        {
            if (_currentFrames == null || _currentFrames.Length <= 1)
            {
                return;
            }

            _elapsed += Time.deltaTime;
            float frameDuration = 1f / _currentFrameRate;

            while (_elapsed >= frameDuration)
            {
                _elapsed -= frameDuration;
                if (loop)
                {
                    _frameIndex = (_frameIndex + 1) % _currentFrames.Length;
                }
                else
                {
                    _frameIndex = Mathf.Min(
                        _frameIndex + 1,
                        _currentFrames.Length - 1);
                }

                _spriteRenderer.sprite = _currentFrames[_frameIndex];
                if (_isPlayingDeath)
                {
                    AlignCurrentFrameToDeathGround();
                }
            }
        }

        private float GetSpriteBottomY()
        {
            Sprite sprite = _spriteRenderer.sprite;
            if (sprite == null)
            {
                return transform.localPosition.y;
            }

            return transform.localPosition.y
                + sprite.bounds.min.y * transform.localScale.y;
        }

        private void AlignCurrentFrameToDeathGround()
        {
            Sprite sprite = _spriteRenderer.sprite;
            if (sprite == null)
            {
                return;
            }

            Vector3 localPosition = transform.localPosition;
            localPosition.y = _deathGroundY
                - sprite.bounds.min.y * transform.localScale.y;
            transform.localPosition = localPosition;
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
