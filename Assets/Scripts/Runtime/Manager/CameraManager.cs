using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

namespace MoonRabbitRush
{
    public class CameraMaanger : MonoBehaviour
    {
        [Header("Shake")]
        [SerializeField, Min(0.01f)] private float _defaultFrequency = 20f;

        public Camera MainCamera { get; private set; }

        private CinemachineCamera _playerCamera;
        private CinemachineFollow _follow;
        private Vector3 _fallbackLocalPosition;
        private Vector3 _baseFollowOffset;
        private float _shakeDurationRemaining;
        private float _shakeAmplitude;
        private float _shakeFrequency;
        private float _shakeTime;

        private void Awake()
        {
            MainCamera = GetComponent<Camera>();

            if (MainCamera == null)
            {
                MainCamera = GetComponentInChildren<Camera>();
            }

            _fallbackLocalPosition = transform.localPosition;
            ResolveShakeTarget();
        }

        private void LateUpdate()
        {
            ResolveShakeTarget();

            if (_shakeDurationRemaining <= 0f)
            {
                ResetShake();
                return;
            }

            _shakeDurationRemaining -= Time.deltaTime;
            _shakeTime += Time.deltaTime * _shakeFrequency;

            float offsetX = (Mathf.PerlinNoise(_shakeTime, 0f) - 0.5f) * 2f;
            float offsetY = (Mathf.PerlinNoise(0f, _shakeTime) - 0.5f) * 2f;
            Vector3 offset = new(offsetX, offsetY, 0f);
            offset *= _shakeAmplitude;

            if (_follow != null)
            {
                _follow.FollowOffset = _baseFollowOffset + offset;
                return;
            }

            transform.localPosition = _fallbackLocalPosition + offset;
        }

        public void PlayShake(
            float duration,
            float amplitude,
            float frequency = -1f)
        {
            if (duration <= 0f || amplitude <= 0f)
            {
                return;
            }

            ResolveShakeTarget();

            _shakeDurationRemaining = Mathf.Max(_shakeDurationRemaining, duration);
            _shakeAmplitude = Mathf.Max(_shakeAmplitude, amplitude);
            _shakeFrequency = Mathf.Max(
                _shakeFrequency,
                frequency > 0f ? frequency : _defaultFrequency);
        }

        public async UniTask ZoomInAsync(
            float sizeMultiplier,
            float duration,
            CancellationToken cancellationToken)
        {
            ResolveShakeTarget();
            if (_playerCamera == null)
            {
                return;
            }

            float startSize = _playerCamera.Lens.OrthographicSize;
            float targetSize = startSize * Mathf.Clamp(sizeMultiplier, 0.1f, 1f);

            if (duration <= 0f)
            {
                _playerCamera.Lens.OrthographicSize = targetSize;
                return;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                cancellationToken.ThrowIfCancellationRequested();
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);
                _playerCamera.Lens.OrthographicSize = Mathf.Lerp(
                    startSize,
                    targetSize,
                    easedProgress);
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            _playerCamera.Lens.OrthographicSize = targetSize;
        }

        private void ResolveShakeTarget()
        {
            if (_follow != null)
            {
                return;
            }

            _playerCamera = FindAnyObjectByType<CinemachineCamera>();
            if (_playerCamera == null)
            {
                return;
            }

            _follow = _playerCamera.GetComponent<CinemachineFollow>();
            if (_follow != null)
            {
                _baseFollowOffset = _follow.FollowOffset;
            }
        }

        private void ResetShake()
        {
            _shakeDurationRemaining = 0f;
            _shakeAmplitude = 0f;
            _shakeFrequency = _defaultFrequency;
            _shakeTime = 0f;

            if (_follow != null)
            {
                _follow.FollowOffset = _baseFollowOffset;
                return;
            }

            transform.localPosition = _fallbackLocalPosition;
        }
    }
}
