using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MoonRabbitRush.Enemies
{
    [RequireComponent(typeof(EnemyHealth))]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class EnemyHitFeedback : MonoBehaviour
    {
        [SerializeField] private Color _hitColor = Color.white;
        [SerializeField] private Color _deathColor = new(0.25f, 0.25f, 0.25f, 1f);
        [SerializeField, Min(0f)] private float _hitFlashDuration = 0.08f;

        private EnemyHealth _health;
        private SpriteRenderer _spriteRenderer;
        private Color _baseColor;
        private CancellationTokenSource _flashCts;

        private void Awake()
        {
            _health = GetComponent<EnemyHealth>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _baseColor = _spriteRenderer.color;
        }

        private void OnEnable()
        {
            _spriteRenderer.color = _baseColor;
            _health.Damaged += PlayHitFlash;
            _health.Died += PlayDeathFeedback;
        }

        private void OnDisable()
        {
            _health.Damaged -= PlayHitFlash;
            _health.Died -= PlayDeathFeedback;
            CancelFlashTask();
            _spriteRenderer.color = _baseColor;
        }

        private void PlayHitFlash(float _)
        {
            CancelFlashTask();
            _flashCts = new CancellationTokenSource();
            PlayHitFlashAsync(_flashCts.Token).Forget();
        }

        private async UniTaskVoid PlayHitFlashAsync(
            CancellationToken cancellationToken)
        {
            try
            {
                _spriteRenderer.color = _hitColor;
                await UniTask.Delay(
                    TimeSpan.FromSeconds(_hitFlashDuration),
                    DelayType.DeltaTime,
                    PlayerLoopTiming.Update,
                    cancellationToken);
                _spriteRenderer.color = _baseColor;
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void PlayDeathFeedback()
        {
            CancelFlashTask();
            _spriteRenderer.color = _deathColor;
        }

        private void CancelFlashTask()
        {
            if (_flashCts == null)
            {
                return;
            }

            _flashCts.Cancel();
            _flashCts.Dispose();
            _flashCts = null;
        }
    }
}
