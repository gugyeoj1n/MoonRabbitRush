using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MoonRabbitRush.Combat
{
    public sealed class TimedEffect : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _duration = 0.5f;
        [SerializeField] private PoolType _poolType;

        private CancellationTokenSource _releaseCts;
        private Animator _animator;
        private bool _isReleased;

        public event Action<TimedEffect> Released;
        public PoolType PoolKey => _poolType;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            if (_animator != null)
            {
                _animator.Rebind();
                _animator.Update(0f);
            }

            _isReleased = false;
            CancelReleaseTask();
            _releaseCts = new CancellationTokenSource();
            ReleaseAfterDurationAsync(_releaseCts.Token).Forget();
        }

        private void OnDisable()
        {
            CancelReleaseTask();
        }

        public void Release()
        {
            if (_isReleased)
            {
                return;
            }

            _isReleased = true;

            if (Released != null)
            {
                Released.Invoke(this);
                return;
            }

            PoolingManager.Release(_poolType, gameObject);
        }

        private async UniTaskVoid ReleaseAfterDurationAsync(
            CancellationToken cancellationToken)
        {
            try
            {
                await UniTask.Delay(
                    TimeSpan.FromSeconds(_duration),
                    DelayType.UnscaledDeltaTime,
                    PlayerLoopTiming.Update,
                    cancellationToken);
                Release();
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void CancelReleaseTask()
        {
            if (_releaseCts == null)
            {
                return;
            }

            _releaseCts.Cancel();
            _releaseCts.Dispose();
            _releaseCts = null;
        }
    }
}
