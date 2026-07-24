using System.Collections;
using UnityEngine;

namespace MoonRabbitRush.Player
{
    [RequireComponent(typeof(PlayerHealth))]
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
        private Coroutine _feedbackRoutine;

        private void Awake()
        {
            _health = GetComponent<PlayerHealth>();
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
            if (_feedbackRoutine != null)
            {
                StopCoroutine(_feedbackRoutine);
            }

            _feedbackRoutine = StartCoroutine(HitFeedbackRoutine());
        }

        private IEnumerator HitFeedbackRoutine()
        {
            _spriteRenderer.color = _hitColor;
            yield return new WaitForSeconds(_hitColorDuration);

            bool isDimmed = false;

            while (_health.IsInvincible)
            {
                isDimmed = !isDimmed;
                Color color = _baseColor;
                color.a = isDimmed ? _invincibleAlpha : _baseColor.a;
                _spriteRenderer.color = color;
                yield return new WaitForSeconds(_blinkInterval);
            }

            _spriteRenderer.color = _baseColor;
            _feedbackRoutine = null;
        }

        private void StopFeedback()
        {
            if (_feedbackRoutine != null)
            {
                StopCoroutine(_feedbackRoutine);
                _feedbackRoutine = null;
            }

            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _baseColor;
            }
        }

        private void PlayDeathFeedback()
        {
            if (_feedbackRoutine != null)
            {
                StopCoroutine(_feedbackRoutine);
                _feedbackRoutine = null;
            }

            _spriteRenderer.color = _deathColor;
        }
    }
}
