using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MoonRabbitRush.Enemies
{
    [RequireComponent(typeof(EnemyHealth))]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class EnemyDeathSpriteAnimation : MonoBehaviour
    {
        [SerializeField] private Sprite[] _deathFrames;
        [SerializeField] private Animator _animator;

        private EnemyHealth _health;
        private EnemyActor _actor;
        private SpriteRenderer _spriteRenderer;
        private SpriteRenderer _shadowRenderer;
        private Color _shadowBaseColor;
        private CancellationTokenSource _playCts;

        private void Awake()
        {
            _health = GetComponent<EnemyHealth>();
            _actor = GetComponent<EnemyActor>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            Transform shadow = transform.Find("Ground Shadow");
            if (shadow != null)
            {
                _shadowRenderer = shadow.GetComponent<SpriteRenderer>();
                if (_shadowRenderer != null)
                {
                    _shadowBaseColor = _shadowRenderer.color;
                }
            }

            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }
        }

        private void OnEnable()
        {
            SetShadowAlpha(_shadowBaseColor.a);

            if (_animator != null)
            {
                _animator.enabled = true;
                _animator.Rebind();
                _animator.Update(0f);
            }

            _health.Died += PlayDeathAnimation;
        }

        private void OnDisable()
        {
            _health.Died -= PlayDeathAnimation;
            CancelPlayTask();
        }

        private void PlayDeathAnimation()
        {
            if (_deathFrames == null || _deathFrames.Length == 0)
            {
                return;
            }

            if (_animator != null)
            {
                _animator.enabled = false;
            }

            CancelPlayTask();
            _playCts = new CancellationTokenSource();
            PlayDeathAnimationAsync(_playCts.Token).Forget();
        }

        private async UniTaskVoid PlayDeathAnimationAsync(
            CancellationToken cancellationToken)
        {
            float duration = _actor != null
                ? Mathf.Max(0.01f, _actor.DeathFeedbackDuration)
                : 0.2f;
            float elapsed = 0f;
            int lastFrameIndex = -1;

            while (elapsed < duration && !cancellationToken.IsCancellationRequested)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                int frameIndex = Mathf.Min(
                    _deathFrames.Length - 1,
                    Mathf.FloorToInt(progress * _deathFrames.Length));

                if (frameIndex != lastFrameIndex)
                {
                    _spriteRenderer.sprite = _deathFrames[frameIndex];
                    lastFrameIndex = frameIndex;
                }

                SetShadowAlpha(_shadowBaseColor.a * (1f - progress));

                await UniTask.Yield(
                    PlayerLoopTiming.Update,
                    cancellationToken);
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                _spriteRenderer.sprite = _deathFrames[_deathFrames.Length - 1];
                SetShadowAlpha(0f);
            }
        }

        private void SetShadowAlpha(float alpha)
        {
            if (_shadowRenderer == null)
            {
                return;
            }

            Color color = _shadowBaseColor;
            color.a = alpha;
            _shadowRenderer.color = color;
        }

        private void CancelPlayTask()
        {
            if (_playCts == null)
            {
                return;
            }

            _playCts.Cancel();
            _playCts.Dispose();
            _playCts = null;
        }
    }
}
