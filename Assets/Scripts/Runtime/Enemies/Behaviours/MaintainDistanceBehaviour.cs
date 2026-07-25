using UnityEngine;

namespace MoonRabbitRush.Enemies
{
    [RequireComponent(typeof(EnemyMotor))]
    public sealed class MaintainDistanceBehaviour : EnemyBehaviour
    {
        [SerializeField, Min(0f)] private float _retreatDistance = 3f;
        [SerializeField, Min(0f)] private float _approachDistance = 6f;

        private EnemyMotor _motor;

        private void Awake()
        {
            _motor = GetComponent<EnemyMotor>();
        }

        private void Update()
        {
            if (Target == null)
            {
                _motor.SetDirection(Vector2.zero);
                return;
            }

            Vector2 toTarget = Target.position - transform.position;
            float distance = toTarget.magnitude;

            if (distance > _approachDistance)
            {
                _motor.SetDirection(toTarget.normalized);
            }
            else if (distance < _retreatDistance)
            {
                _motor.SetDirection(-toTarget.normalized);
            }
            else
            {
                _motor.SetDirection(Vector2.zero);
            }
        }

        private void OnValidate()
        {
            _retreatDistance = Mathf.Max(0f, _retreatDistance);
            _approachDistance = Mathf.Max(_retreatDistance, _approachDistance);
        }

        private void OnDisable()
        {
            _motor?.SetDirection(Vector2.zero);
        }
    }
}
