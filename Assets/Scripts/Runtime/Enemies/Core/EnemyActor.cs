using System.Collections;
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
        [SerializeField, Min(0f)] private float _deathFeedbackDuration = 0.2f;

        private EnemyHealth _health;
        private EnemyMotor _motor;
        private EnemyBehaviour[] _behaviours;
        private Collider2D[] _colliders;
        private Coroutine _deactivateRoutine;
        private PlayerLootCollector _lootCollector;
        private bool _isInitialized;

        public bool IsActive =>
            gameObject.activeInHierarchy && _health != null && _health.IsAlive;
        public EnemyHealth Health => _health;

        private void Awake()
        {
            _health = GetComponent<EnemyHealth>();
            _motor = GetComponent<EnemyMotor>();
            _behaviours = GetComponents<EnemyBehaviour>();
            _colliders = GetComponents<Collider2D>();
            _health.Died += HandleDeath;
        }

        private void OnDestroy()
        {
            EnemyRegistry.Unregister(this);

            if (_health != null)
            {
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

            if (_deactivateRoutine != null)
            {
                StopCoroutine(_deactivateRoutine);
                _deactivateRoutine = null;
            }

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

            _deactivateRoutine = StartCoroutine(DeactivateAfterFeedback());
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

        private IEnumerator DeactivateAfterFeedback()
        {
            yield return new WaitForSeconds(_deathFeedbackDuration);
            _deactivateRoutine = null;
            Deactivate();
        }
    }
}
