using System.Collections;
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
        private Coroutine _flashRoutine;

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

            if (_flashRoutine != null)
            {
                StopCoroutine(_flashRoutine);
                _flashRoutine = null;
            }
        }

        private void PlayHitFlash(float _)
        {
            if (_flashRoutine != null)
            {
                StopCoroutine(_flashRoutine);
            }

            _flashRoutine = StartCoroutine(HitFlashRoutine());
        }

        private IEnumerator HitFlashRoutine()
        {
            _spriteRenderer.color = _hitColor;
            yield return new WaitForSeconds(_hitFlashDuration);
            _spriteRenderer.color = _baseColor;
            _flashRoutine = null;
        }

        private void PlayDeathFeedback()
        {
            if (_flashRoutine != null)
            {
                StopCoroutine(_flashRoutine);
                _flashRoutine = null;
            }

            _spriteRenderer.color = _deathColor;
        }
    }
}
