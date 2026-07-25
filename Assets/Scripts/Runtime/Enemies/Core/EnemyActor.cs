using System.Collections;
using UnityEngine;

namespace MoonRabbitRush.Enemies
{
    [RequireComponent(typeof(EnemyHealth))]
    [RequireComponent(typeof(EnemyMotor))]
    public sealed class EnemyActor : MonoBehaviour, IEnemy
    {
        [SerializeField] private EnemyStatsData _stats;
        [SerializeField, Min(0f)] private float _deathFeedbackDuration = 0.2f;

        private EnemyHealth _health;
        private EnemyMotor _motor;
        private EnemyBehaviour[] _behaviours;
        private Collider2D[] _colliders;
        private Coroutine _deactivateRoutine;
        private bool _isInitialized;

        public bool IsActive =>
            gameObject.activeInHierarchy && _health != null && _health.IsAlive;

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
            _motor.Stop();
            gameObject.SetActive(false);
        }

        private void HandleDeath()
        {
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

        private IEnumerator DeactivateAfterFeedback()
        {
            yield return new WaitForSeconds(_deathFeedbackDuration);
            _deactivateRoutine = null;
            Deactivate();
        }
    }
}
