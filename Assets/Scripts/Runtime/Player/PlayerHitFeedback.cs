using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MoonRabbitRush.Player
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PlayerHitFeedback : MonoBehaviour
    {
        [SerializeField] private Color _hitColor = new(1f, 0.25f, 0.25f, 1f);
        [SerializeField] private Color _deathColor = new(0.3f, 0.3f, 0.3f, 1f);
        [SerializeField, Min(0f)] private float _hitColorDuration = 0.08f;
        [SerializeField, Min(0.01f)] private float _blinkInterval = 0.08f;
        [SerializeField, Range(0f, 1f)] private float _invincibleAlpha = 0.35f;

        private PlayerHealth _health;
        private SpriteRenderer _spriteRenderer;
        private Color _baseColor;
        private CancellationTokenSource _feedbackCts;

        private void Awake()
        {
            _health = GetComponentInParent<PlayerHealth>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _baseColor = _spriteRenderer.color;
        }

        private void OnEnable()
        {
            _health.Damaged += PlayHitFeedback;
            _health.Died += PlayDeathFeedback;
        }

        private void OnDisable()
        {
            _health.Damaged -= PlayHitFeedback;
            _health.Died -= PlayDeathFeedback;
            StopFeedback();
        }

        private void PlayHitFeedback(float _)
        {
            CancelFeedbackTask();
            _feedbackCts = new CancellationTokenSource();
            PlayHitFeedbackAsync(_feedbackCts.Token).Forget();
        }

        private async UniTaskVoid PlayHitFeedbackAsync(
            CancellationToken cancellationToken)
        {
            try
            {
                _spriteRenderer.color = _hitColor;
                await UniTask.Delay(
                    TimeSpan.FromSeconds(_hitColorDuration),
                    DelayType.DeltaTime,
                    PlayerLoopTiming.Update,
                    cancellationToken);

                bool isDimmed = false;

                while (_health.IsInvincible)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    isDimmed = !isDimmed;
                    Color color = _baseColor;
                    color.a = isDimmed ? _invincibleAlpha : _baseColor.a;
                    _spriteRenderer.color = color;
                    await UniTask.Delay(
                        TimeSpan.FromSeconds(_blinkInterval),
                        DelayType.DeltaTime,
                        PlayerLoopTiming.Update,
                        cancellationToken);
                }

                _spriteRenderer.color = _baseColor;
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void StopFeedback()
        {
            CancelFeedbackTask();

            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _baseColor;
            }
        }

        private void PlayDeathFeedback()
        {
            CancelFeedbackTask();
            _spriteRenderer.color = _deathColor;
        }

        private void CancelFeedbackTask()
        {
            if (_feedbackCts == null)
            {
                return;
            }

            _feedbackCts.Cancel();
            _feedbackCts.Dispose();
            _feedbackCts = null;
        }
    }
}
