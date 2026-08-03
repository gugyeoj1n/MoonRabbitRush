using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MoonRabbitRush.Enemies.Bosses
{
    public sealed class BossPatternController : EnemyBehaviour
    {
        [SerializeField] private BossAttackPattern[] _patterns;
        [SerializeField, Min(0f)] private float _initialDelay = 0.8f;
        [SerializeField, Min(0f)] private float _patternInterval = 0.7f;

        private CancellationTokenSource _patternCts;
        private EnemyAnimationController _animationController;

        public override void Initialize(
            Transform target,
            EnemyStatsData stats)
        {
            base.Initialize(target, stats);
            _animationController ??= GetComponent<EnemyAnimationController>();

            if (_patterns == null || _patterns.Length == 0)
            {
                _patterns = GetComponents<BossAttackPattern>();
            }

            CancelPatternLoop();
            _patternCts = new CancellationTokenSource();
            RunPatternsAsync(_patternCts.Token).Forget();
        }

        private void OnDisable()
        {
            CancelPatternLoop();
        }

        private async UniTaskVoid RunPatternsAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_initialDelay > 0f)
                {
                    await UniTask.Delay(
                        TimeSpan.FromSeconds(_initialDelay),
                        DelayType.DeltaTime,
                        PlayerLoopTiming.Update,
                        cancellationToken);
                }

                int patternIndex = 0;

                while (enabled && gameObject.activeInHierarchy)
                {
                    if (_patterns == null || _patterns.Length == 0)
                    {
                        return;
                    }

                    BossAttackPattern pattern = _patterns[patternIndex];
                    patternIndex = (patternIndex + 1) % _patterns.Length;

                    if (pattern != null && pattern.IsReady)
                    {
                        _animationController?.PlayAttack();
                        await pattern.ExecuteAsync(cancellationToken);
                    }

                    if (_patternInterval > 0f)
                    {
                        await UniTask.Delay(
                            TimeSpan.FromSeconds(_patternInterval),
                            DelayType.DeltaTime,
                            PlayerLoopTiming.Update,
                            cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void OnValidate()
        {
            _initialDelay = Mathf.Max(0f, _initialDelay);
            _patternInterval = Mathf.Max(0f, _patternInterval);
        }

        private void CancelPatternLoop()
        {
            if (_patternCts == null)
            {
                return;
            }

            _patternCts.Cancel();
            _patternCts.Dispose();
            _patternCts = null;
        }
    }
}
