using UnityEngine;

namespace MoonRabbitRush.Enemies
{
    [RequireComponent(typeof(EnemyMotor))]
    public sealed class ChaseTargetBehaviour : EnemyBehaviour
    {
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

            Vector2 direction = Target.position - transform.position;
            _motor.SetDirection(direction.normalized);
        }

        private void OnDisable()
        {
            _motor?.SetDirection(Vector2.zero);
        }
    }
}
