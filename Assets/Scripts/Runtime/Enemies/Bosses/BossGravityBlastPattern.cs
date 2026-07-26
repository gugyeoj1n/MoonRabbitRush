using System.Collections;
using MoonRabbitRush.Combat;
using MoonRabbitRush.Player;
using UnityEngine;

namespace MoonRabbitRush.Enemies.Bosses
{
    public sealed class BossGravityBlastPattern : BossAttackPattern
    {
        [SerializeField, Min(0f)] private float _radius = 6f;
        [SerializeField, Min(0.05f)] private float _chargeDuration = 2.5f;
        [SerializeField, Min(0f)] private float _resistanceSpeed = 1.2f;
        [SerializeField] private Sprite _outlineSprite;
        [SerializeField] private Sprite _fillSprite;
        [SerializeField, Range(0.1f, 1f)] private float _verticalScale = 0.72f;
        [SerializeField] private Color _outlineColor =
            new Color32(255, 83, 83, 204);
        [SerializeField] private Color _fillColor =
            new Color32(255, 129, 129, 115);

        private PlayerMovement _targetMovement;

        public override void Initialize(
            Transform target,
            EnemyStatsData stats)
        {
            base.Initialize(target, stats);
            _targetMovement = target.GetComponent<PlayerMovement>();
        }

        public override IEnumerator Execute()
        {
            var telegraphObject =
                new GameObject("Boss Gravity Telegraph");
            CircleTelegraphView telegraph =
                telegraphObject.AddComponent<CircleTelegraphView>();
            telegraph.Initialize(
                transform.position,
                _radius,
                _chargeDuration,
                _outlineSprite,
                _fillSprite,
                _outlineColor,
                _fillColor,
                _verticalScale);

            float elapsed = 0f;

            while (elapsed < _chargeDuration)
            {
                elapsed += Time.deltaTime;
                Vector2 targetPosition = Target.position;
                Vector2 toBoss = (Vector2)transform.position - targetPosition;
                float distance = toBoss.magnitude;

                if (distance > Mathf.Epsilon && distance <= _radius)
                {
                    _targetMovement?.SetExternalVelocity(
                        toBoss.normalized * _resistanceSpeed);
                }
                else
                {
                    ClearMovementResistance();
                }

                yield return null;
            }

            ClearMovementResistance();

            if (TargetDamageable.IsAlive &&
                Vector2.Distance(Target.position, transform.position) <= _radius)
            {
                TargetDamageable.TakeDamage(
                    new DamageInfo(
                        Stats.AttackDamage,
                        Target.position,
                        gameObject));
            }
        }

        private void OnDisable()
        {
            ClearMovementResistance();
        }

        private void ClearMovementResistance()
        {
            _targetMovement?.SetExternalVelocity(Vector2.zero);
        }

        private void OnValidate()
        {
            _radius = Mathf.Max(0f, _radius);
            _chargeDuration = Mathf.Max(0.05f, _chargeDuration);
            _resistanceSpeed = Mathf.Max(0f, _resistanceSpeed);
            _verticalScale = Mathf.Clamp(_verticalScale, 0.1f, 1f);
        }
    }
}
