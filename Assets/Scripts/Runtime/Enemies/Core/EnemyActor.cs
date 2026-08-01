using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MoonRabbitRush.Combat;
using MoonRabbitRush.Progression;
using UnityEngine;

namespace MoonRabbitRush.Enemies
{
    [RequireComponent(typeof(EnemyHealth))]
    [RequireComponent(typeof(EnemyMotor))]
    public sealed class EnemyActor : MonoBehaviour, IEnemy
    {
        [SerializeField] private EnemyStatsData _stats;
        [SerializeField] private ExperienceDrop _experienceDropPrefab;

        [Header("Death Feedback")]
        [SerializeField, Min(0f)] private float _deathFeedbackDuration = 0.2f;
        [SerializeField, Min(0f)] private float _deathHoldDuration = 0.2f;

        [Header("Hit Reaction")]
        [SerializeField] private bool _receivesHitReaction = true;
        [SerializeField, Min(0f)] private float _hitKnockbackDistance = 0.12f;
        [SerializeField, Min(0.01f)] private float _hitReactionDuration = 0.08f;

        private EnemyHealth _health;
        private EnemyMotor _motor;
        private EnemyBehaviour[] _behaviours;
        private Collider2D[] _colliders;
        private CancellationTokenSource _deactivateCts;
        private PlayerLootCollector _lootCollector;
        private bool _isInitialized;

        public bool IsActive =>
            gameObject.activeInHierarchy && _health != null && _health.IsAlive;
        public EnemyHealth Health => _health;
        public Transform Target { get; private set; }
        public float DeathFeedbackDuration => _deathFeedbackDuration;
        public float DeathDeactivationDelay =>
            _deathFeedbackDuration + _deathHoldDuration;

        private void Awake()
        {
            _health = GetComponent<EnemyHealth>();
            _motor = GetComponent<EnemyMotor>();
            _behaviours = GetComponents<EnemyBehaviour>();
            _colliders = GetComponents<Collider2D>();
            _health.DamageReceived += HandleDamageReceived;
            _health.Died += HandleDeath;
        }

        private void OnDestroy()
        {
            EnemyRegistry.Unregister(this);

            if (_health != null)
            {
                _health.DamageReceived -= HandleDamageReceived;
                _health.Died -= HandleDeath;
            }
        }

        public void Initialize(Transform target)
        {
            if (_stats == null)
            {
                Debug.LogError($"{nameof(EnemyStatsData)} is not assigned.", this);
                return;
            }

            if (target == null)
            {
                Debug.LogError("Enemy target is not assigned.", this);
                return;
            }

            _health.Initialize(_stats);
            _motor.Initialize(_stats);
            Target = target;
            _lootCollector = target.GetComponent<PlayerLootCollector>();

            foreach (EnemyBehaviour behaviour in _behaviours)
            {
                behaviour.Initialize(target, _stats);
            }

            _isInitialized = true;
            Activate();
        }

        public void Activate()
        {
            if (!_isInitialized)
            {
                return;
            }

            CancelDeactivateTask();

            gameObject.SetActive(true);
            _health.ResetHealth();
            _motor.Resume();
            EnemyRegistry.Register(this);

            foreach (Collider2D enemyCollider in _colliders)
            {
                enemyCollider.enabled = true;
            }

            foreach (EnemyBehaviour behaviour in _behaviours)
            {
                behaviour.enabled = true;
            }
        }

        public void Deactivate()
        {
            EnemyRegistry.Unregister(this);
            _motor.Stop();
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            CancelDeactivateTask();
            EnemyRegistry.Unregister(this);
        }

        private void HandleDeath()
        {
            DropExperience();
            _motor.Stop();

            foreach (Collider2D enemyCollider in _colliders)
            {
                enemyCollider.enabled = false;
            }

            foreach (EnemyBehaviour behaviour in _behaviours)
            {
                behaviour.enabled = false;
            }

            CancelDeactivateTask();
            _deactivateCts = new CancellationTokenSource();
            DeactivateAfterFeedbackAsync(_deactivateCts.Token).Forget();
        }

        private void DropExperience()
        {
            if (_experienceDropPrefab == null)
            {
                Debug.LogError("Experience drop prefab is not assigned.", this);
                return;
            }

            ExperienceDrop experienceDrop = Instantiate(
                _experienceDropPrefab,
                transform.position,
                Quaternion.identity);
            experienceDrop.Initialize(_lootCollector, _stats.ExperienceReward);
        }

        private void HandleDamageReceived(DamageInfo damage)
        {
            if (!_receivesHitReaction || !_health.IsAlive)
            {
                return;
            }

            Vector2 direction = ResolveHitDirection(damage);
            _motor.ApplyHitReaction(
                direction,
                _hitKnockbackDistance,
                _hitReactionDuration);
        }

        private Vector2 ResolveHitDirection(DamageInfo damage)
        {
            Vector2 origin = damage.Source != null
                ? damage.Source.transform.position
                : damage.HitPoint;
            Vector2 direction = (Vector2)transform.position - origin;

            if (direction.sqrMagnitude > Mathf.Epsilon)
            {
                return direction.normalized;
            }

            return _motor.MoveDirection == Vector2.zero
                ? Vector2.up
                : -_motor.MoveDirection;
        }

        private async UniTaskVoid DeactivateAfterFeedbackAsync(
            CancellationToken cancellationToken)
        {
            try
            {
                await UniTask.Delay(
                    TimeSpan.FromSeconds(DeathDeactivationDelay),
                    DelayType.DeltaTime,
                    PlayerLoopTiming.Update,
                    cancellationToken);
                Deactivate();
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void CancelDeactivateTask()
        {
            if (_deactivateCts == null)
            {
                return;
            }

            _deactivateCts.Cancel();
            _deactivateCts.Dispose();
            _deactivateCts = null;
        }
    }
}
