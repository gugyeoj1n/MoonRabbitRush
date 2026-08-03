using UnityEngine;

namespace MoonRabbitRush.Enemies
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(EnemyMotor))]
    public sealed class EnemyAnimationController : MonoBehaviour
    {
        private static readonly int IsMovingHash =
            Animator.StringToHash("IsMoving");
        private static readonly int AttackHash =
            Animator.StringToHash("Attack");

        [SerializeField, Min(0f)] private float _moveThreshold = 0.01f;

        private Animator _animator;
        private EnemyMotor _motor;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _motor = GetComponent<EnemyMotor>();
        }

        private void OnEnable()
        {
            _animator.ResetTrigger(AttackHash);
            UpdateMoveState();
        }

        private void Update()
        {
            UpdateMoveState();
        }

        public void PlayAttack()
        {
            if (_animator != null && _animator.isActiveAndEnabled)
            {
                _animator.SetTrigger(AttackHash);
            }
        }

        private void UpdateMoveState()
        {
            if (_animator == null || !_animator.isActiveAndEnabled)
            {
                return;
            }

            float thresholdSquared = _moveThreshold * _moveThreshold;
            _animator.SetBool(
                IsMovingHash,
                _motor != null &&
                _motor.MoveDirection.sqrMagnitude > thresholdSquared);
        }
    }
}
