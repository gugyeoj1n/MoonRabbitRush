using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MoonRabbitRush.Enemies.Bosses
{
    public sealed class BossPatternController : EnemyBehaviour
    {
        [SerializeField] private BossAttackPattern[] _patterns;
        [SerializeField, Min(0f)] private float _initialDelay = 1.4f;
        [SerializeField, Min(0f)] private float _patternInterval = 1.1f;

        [Header("Round Scaling")]
        [SerializeField, Min(0f)]
        private float _initialDelayReductionPerRound = 0.2f;
        [SerializeField, Min(0f)]
        private float _patternIntervalReductionPerRound = 0.15f;
        [SerializeField, Min(0f)] private float _minimumInitialDelay = 0.5f;
        [SerializeField, Min(0f)] private float _minimumPatternInterval = 0.35f;

        private CancellationTokenSource _patternCts;
        private EnemyAnimationController _animationController;
        private int _bossRound = 1;

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

            RestartPatternLoop();
        }

        public void ConfigureRound(int bossRound)
        {
            _bossRound = Mathf.Max(1, bossRound);

            if (enabled && gameObject.activeInHierarchy)
            {
                RestartPatternLoop();
            }
        }

        private void OnDisable()
        {
            CancelPatternLoop();
        }

        private async UniTaskVoid RunPatternsAsync(CancellationToken cancellationToken)
        {
            try
            {
                float initialDelay = Mathf.Max(
                    _minimumInitialDelay,
                    _initialDelay -
                    (_bossRound - 1) * _initialDelayReductionPerRound);
                float patternInterval = Mathf.Max(
                    _minimumPatternInterval,
                    _patternInterval -
                    (_bossRound - 1) * _patternIntervalReductionPerRound);

                if (initialDelay > 0f)
                {
                    await UniTask.Delay(
                        TimeSpan.FromSeconds(initialDelay),
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

                    if (patternInterval > 0f)
                    {
                        await UniTask.Delay(
                            TimeSpan.FromSeconds(patternInterval),
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
            _initialDelayReductionPerRound =
                Mathf.Max(0f, _initialDelayReductionPerRound);
            _patternIntervalReductionPerRound =
                Mathf.Max(0f, _patternIntervalReductionPerRound);
            _minimumInitialDelay = Mathf.Max(0f, _minimumInitialDelay);
            _minimumPatternInterval = Mathf.Max(0f, _minimumPatternInterval);
        }

        private void RestartPatternLoop()
        {
            CancelPatternLoop();
            _patternCts = new CancellationTokenSource();
            RunPatternsAsync(_patternCts.Token).Forget();
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
