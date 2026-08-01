using UnityEngine;

namespace MoonRabbitRush.Enemies
{
    [RequireComponent(typeof(EnemyMotor))]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class EnemyFacingView : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _horizontalDeadZone = 0.01f;
        [SerializeField] private bool _faceTarget;

        private EnemyActor _actor;
        private EnemyMotor _motor;
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _actor = GetComponent<EnemyActor>();
            _motor = GetComponent<EnemyMotor>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            _spriteRenderer.flipX = false;
        }

        private void LateUpdate()
        {
            float horizontalDirection = _motor.MoveDirection.x;

            if (_faceTarget && _actor != null && _actor.Target != null)
            {
                horizontalDirection =
                    _actor.Target.position.x - transform.position.x;
            }

            if (Mathf.Abs(horizontalDirection) <= _horizontalDeadZone)
            {
                return;
            }

            _spriteRenderer.flipX = horizontalDirection < 0f;
        }

        private void OnValidate()
        {
            _horizontalDeadZone = Mathf.Max(0f, _horizontalDeadZone);
        }
    }
}
