using UnityEngine;

namespace MoonRabbitRush.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class EnemyMotor : MonoBehaviour
    {
        private Rigidbody2D _rigidbody;
        private EnemyStatsData _stats;
        private Vector2 _moveDirection;
        private bool _canMove;

        public Vector2 MoveDirection => _moveDirection;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            if (!_canMove || _stats == null || _moveDirection == Vector2.zero)
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
            _rigidbody.linearVelocity = Vector2.zero;
        }

        public void Resume()
        {
            _canMove = _stats != null;
        }
    }
}
