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
        [SerializeField] private Sprite _outlineSprite;
        [SerializeField] private Sprite _fillSprite;
        [SerializeField, Range(0.1f, 1f)] private float _verticalScale = 0.72f;
        [SerializeField] private Color _outlineColor =
            new Color32(255, 83, 83, 204);
        [SerializeField] private Color _fillColor =
            new Color32(255, 129, 129, 115);

        private Component _damageTarget;
        private EnemyAnimationController _animationController;
        private CircleTelegraphView _activeTelegraph;
        private FallingAreaProjectile _activeProjectile;
        private float _nextAttackTime;

        public override void Initialize(Transform target, EnemyStatsData stats)
        {
            base.Initialize(target, stats);
            _damageTarget = target.GetComponent(typeof(IDamageable)) as Component;
            _animationController ??= GetComponent<EnemyAnimationController>();
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
            _animationController?.PlayAttack();

            _activeTelegraph = CircleTelegraphView.GetFromPool(
                "Circle Telegraph");
            if (_activeTelegraph == null)
            {
                return;
            }

            _activeTelegraph.Released += HandleTelegraphReleased;
            _activeTelegraph.Initialize(
                impactPosition,
                _impactRadius,
                _telegraphDuration,
                _outlineSprite,
                _fillSprite,
                _outlineColor,
                _fillColor,
                _verticalScale);
            if (_activeTelegraph == null)
            {
                return;
            }

            _activeProjectile = GetProjectile(impactPosition);
            if (_activeProjectile == null)
            {
                _activeTelegraph.Release();
                _activeTelegraph = null;
                return;
            }

            _activeProjectile.Released += HandleProjectileReleased;
            _activeProjectile.Launch(
                impactPosition,
                _fallHeight,
                _impactRadius,
                _telegraphDuration,
                Stats.AttackDamage,
                _damageTarget,
                gameObject);
        }

        private void HandleProjectileReleased(FallingAreaProjectile projectile)
        {
            projectile.Released -= HandleProjectileReleased;

            if (_activeProjectile == projectile)
            {
                _activeProjectile = null;
            }

            PoolingManager.Release(
                PoolType.ProjectileOrbitronMissile,
                projectile.gameObject);
        }

        private void HandleTelegraphReleased(CircleTelegraphView telegraph)
        {
            telegraph.Released -= HandleTelegraphReleased;

            if (_activeTelegraph == telegraph)
            {
                _activeTelegraph = null;
            }

            PoolingManager.Release(
                PoolType.TelegraphCircle,
                telegraph.gameObject);
        }

        private FallingAreaProjectile GetProjectile(Vector2 position)
        {
            const PoolType poolType = PoolType.ProjectileOrbitronMissile;
            if (!PoolingManager.IsRegistered(poolType))
            {
                PoolingManager.RegisterPool(
                    poolType,
                    () => Instantiate(_projectilePrefab).gameObject,
                    defaultCapacity: 10,
                    maxSize: 100);
            }

            PoolingManager.GetObject(poolType, out GameObject projectileObject);
            if (projectileObject == null ||
                !projectileObject.TryGetComponent(
                    out FallingAreaProjectile projectile))
            {
                return null;
            }

            projectile.transform.SetPositionAndRotation(
                position,
                Quaternion.identity);
            return projectile;
        }

        private void OnValidate()
        {
            _attackRange = Mathf.Max(0f, _attackRange);
            _impactRadius = Mathf.Max(0f, _impactRadius);
            _fallHeight = Mathf.Max(0f, _fallHeight);
            _telegraphDuration = Mathf.Max(0.05f, _telegraphDuration);
            _verticalScale = Mathf.Clamp(_verticalScale, 0.1f, 1f);
        }
    }
}
