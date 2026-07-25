using UnityEngine;

namespace MoonRabbitRush.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class EnemyMotor : MonoBehaviour
    {
        private Rigidbody2D _rigidbody;
        private EnemyStatsData _stats;
        private Vector2 _moveDirection;
        private Vector2 _hitReactionDirection;
        private float _hitReactionSpeed;
        private float _hitReactionTimeRemaining;
        private bool _canMove;

        public Vector2 MoveDirection => _moveDirection;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            if (!_canMove || _stats == null)
            {
                return;
            }

            if (_hitReactionTimeRemaining > 0f)
            {
                float stepDuration = Mathf.Min(
                    Time.fixedDeltaTime,
                    _hitReactionTimeRemaining);
                _rigidbody.MovePosition(
                    _rigidbody.position +
                    _hitReactionDirection * (_hitReactionSpeed * stepDuration));
                _hitReactionTimeRemaining -= stepDuration;
                return;
            }

            if (_moveDirection == Vector2.zero)
            {
                return;
            }

            Vector2 nextPosition =
                _rigidbody.position +
                _moveDirection * (_stats.MoveSpeed * Time.fixedDeltaTime);

            _rigidbody.MovePosition(nextPosition);
        }

        public void Initialize(EnemyStatsData stats)
        {
            _stats = stats;
            _canMove = _stats != null;
        }

        public void SetDirection(Vector2 direction)
        {
            _moveDirection = _canMove
                ? Vector2.ClampMagnitude(direction, 1f)
                : Vector2.zero;
        }

        public void Stop()
        {
            _canMove = false;
            _moveDirection = Vector2.zero;
            _hitReactionTimeRemaining = 0f;
            _rigidbody.linearVelocity = Vector2.zero;
        }

        public void Resume()
        {
            _canMove = _stats != null;
            _hitReactionTimeRemaining = 0f;
        }

        public void ApplyHitReaction(
            Vector2 direction,
            float knockbackDistance,
            float duration)
        {
            if (!_canMove || direction == Vector2.zero ||
                knockbackDistance <= 0f || duration <= 0f)
            {
                return;
            }

            _hitReactionDirection = direction.normalized;
            _hitReactionSpeed = knockbackDistance / duration;
            _hitReactionTimeRemaining = duration;
            _rigidbody.linearVelocity = Vector2.zero;
        }
    }
}
