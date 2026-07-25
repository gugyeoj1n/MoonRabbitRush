using MoonRabbitRush.Combat;
using UnityEngine;

namespace MoonRabbitRush.Enemies
{
    public sealed class TelegraphedFallingAttack : EnemyBehaviour
    {
        [Header("Payload")]
        [SerializeField] private FallingAreaProjectile _projectilePrefab;
        [SerializeField, Min(0f)] private float _attackRange = 10f;
        [SerializeField, Min(0f)] private float _impactRadius = 2f;
        [SerializeField, Min(0f)] private float _fallHeight = 6f;
        [SerializeField, Min(0.05f)] private float _telegraphDuration = 1.5f;

        [Header("Telegraph")]
        [SerializeField] private Color _telegraphStartColor =
            new(1f, 0.85f, 0.1f, 0.55f);
        [SerializeField] private Color _telegraphEndColor =
            new(1f, 0.1f, 0.05f, 0.95f);

        private Component _damageTarget;
        private CircleTelegraphView _activeTelegraph;
        private FallingAreaProjectile _activeProjectile;
        private float _nextAttackTime;

        public override void Initialize(Transform target, EnemyStatsData stats)
        {
            base.Initialize(target, stats);
            _damageTarget = target.GetComponent(typeof(IDamageable)) as Component;
            _nextAttackTime = Time.time + stats.AttackInterval;

            if (_damageTarget == null)
            {
                Debug.LogError("Attack target must implement IDamageable.", this);
            }
        }

        private void Update()
        {
            if (Target == null ||
                Stats == null ||
                _damageTarget == null ||
                _projectilePrefab == null ||
                _activeProjectile != null ||
                Time.time < _nextAttackTime)
            {
                return;
            }

            Vector2 toTarget = Target.position - transform.position;

            if (toTarget.sqrMagnitude > _attackRange * _attackRange)
            {
                return;
            }

            BeginAttack(Target.position);
        }

        private void OnDisable()
        {
            if (_activeTelegraph != null)
            {
                _activeTelegraph.Release();
                _activeTelegraph = null;
            }

            if (_activeProjectile != null)
            {
                _activeProjectile.Release();
                _activeProjectile = null;
            }
        }

        private void BeginAttack(Vector2 impactPosition)
        {
            _nextAttackTime = Time.time + Stats.AttackInterval;

            var telegraphObject = new GameObject("Circle Telegraph");
            _activeTelegraph = telegraphObject.AddComponent<CircleTelegraphView>();
            _activeTelegraph.Initialize(
                impactPosition,
                _impactRadius,
                _telegraphDuration,
                _telegraphStartColor,
                _telegraphEndColor);

            _activeProjectile = Instantiate(
                _projectilePrefab,
                impactPosition,
                Quaternion.identity);
            _activeProjectile.Launch(
                impactPosition,
                _fallHeight,
                _impactRadius,
                _telegraphDuration,
                Stats.AttackDamage,
                _damageTarget,
                gameObject);
        }

        private void OnValidate()
        {
            _attackRange = Mathf.Max(0f, _attackRange);
            _impactRadius = Mathf.Max(0f, _impactRadius);
            _fallHeight = Mathf.Max(0f, _fallHeight);
            _telegraphDuration = Mathf.Max(0.05f, _telegraphDuration);
        }
    }
}
