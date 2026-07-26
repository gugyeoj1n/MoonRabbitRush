using System.Collections;
using UnityEngine;

namespace MoonRabbitRush.Enemies.Bosses
{
    public sealed class BossPatternController : EnemyBehaviour
    {
        [SerializeField] private BossAttackPattern[] _patterns;
        [SerializeField, Min(0f)] private float _initialDelay = 0.8f;
        [SerializeField, Min(0f)] private float _patternInterval = 0.7f;

        private Coroutine _patternRoutine;

        public override void Initialize(
            Transform target,
            EnemyStatsData stats)
        {
            base.Initialize(target, stats);

            if (_patterns == null || _patterns.Length == 0)
            {
                _patterns = GetComponents<BossAttackPattern>();
            }

            if (_patternRoutine != null)
            {
                StopCoroutine(_patternRoutine);
            }

            _patternRoutine = StartCoroutine(RunPatterns());
        }

        private void OnDisable()
        {
            if (_patternRoutine != null)
            {
                StopCoroutine(_patternRoutine);
                _patternRoutine = null;
            }
        }

        private IEnumerator RunPatterns()
        {
            if (_initialDelay > 0f)
            {
                yield return new WaitForSeconds(_initialDelay);
            }

            int patternIndex = 0;

            while (enabled && gameObject.activeInHierarchy)
            {
                if (_patterns == null || _patterns.Length == 0)
                {
                    yield break;
                }

                BossAttackPattern pattern = _patterns[patternIndex];
                patternIndex = (patternIndex + 1) % _patterns.Length;

                if (pattern != null && pattern.IsReady)
                {
                    yield return pattern.Execute();
                }

                if (_patternInterval > 0f)
                {
                    yield return new WaitForSeconds(_patternInterval);
                }
            }
        }

        private void OnValidate()
        {
            _initialDelay = Mathf.Max(0f, _initialDelay);
            _patternInterval = Mathf.Max(0f, _patternInterval);
        }
    }
}
