using UnityEngine;
using UnityEngine.InputSystem;

namespace MoonRabbitRush.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerInput))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        private const string MoveActionName = "Move";

        [SerializeField] private PlayerStatsData _stats;

        private Rigidbody2D _rigidbody;
        private InputAction _moveAction;
        private Vector2 _moveInput;
        private bool _canMove = true;

        public Vector2 MoveInput => _moveInput;
        public bool CanMove => _canMove;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _moveAction = GetComponent<PlayerInput>().actions?.FindAction(
                MoveActionName,
                throwIfNotFound: false);

            if (_stats == null)
            {
                Debug.LogError($"{nameof(PlayerStatsData)} is not assigned.", this);
            }

            if (_moveAction == null)
            {
                Debug.LogError($"Input action '{MoveActionName}' was not found.", this);
            }
        }

        private void OnEnable()
        {
            _moveAction?.Enable();
        }

        private void Update()
        {
            _moveInput = _canMove && _moveAction != null
                ? Vector2.ClampMagnitude(_moveAction.ReadValue<Vector2>(), 1f)
                : Vector2.zero;
        }

        private void FixedUpdate()
        {
            if (_stats == null || _moveInput == Vector2.zero)
            {
                return;
            }

            Vector2 nextPosition =
                _rigidbody.position + _moveInput * (_stats.MoveSpeed * Time.fixedDeltaTime);

            _rigidbody.MovePosition(nextPosition);
        }

        public void SetMovementEnabled(bool isEnabled)
        {
            _canMove = isEnabled;

            if (!isEnabled)
            {
                _moveInput = Vector2.zero;
                _rigidbody.linearVelocity = Vector2.zero;
            }
        }
    }
}
