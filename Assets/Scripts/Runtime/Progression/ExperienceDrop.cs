using System;
using UnityEngine;

namespace MoonRabbitRush.Progression
{
    public sealed class ExperienceDrop : MonoBehaviour
    {
        [SerializeField] private Transform _visualTransform;

        [Header("Attraction")]
        [SerializeField, Min(0.1f)] private float _initialSpeed = 3f;
        [SerializeField, Min(0f)] private float _acceleration = 18f;
        [SerializeField, Min(0.1f)] private float _maximumSpeed = 14f;
        [SerializeField, Min(0.01f)] private float _collectDistance = 0.15f;

        [Header("Idle Motion")]
        [SerializeField, Min(0f)] private float _bobHeight = 0.06f;
        [SerializeField, Min(0f)] private float _bobSpeed = 3f;

        private PlayerLootCollector _collector;
        private float _moveSpeed;
        private float _bobOffset;
        private int _experienceAmount;
        private bool _isAttracted;
        private bool _isReleased;

        public event Action<ExperienceDrop> Released;

        private void Update()
        {
            if (_isReleased || _collector == null || !_collector.CanCollect)
            {
                return;
            }

            if (!_isAttracted)
            {
                UpdateIdle();

                if (((Vector2)transform.position - _collector.Position).sqrMagnitude <=
                    _collector.LootRadius * _collector.LootRadius)
                {
                    _isAttracted = true;
                    _moveSpeed = _initialSpeed;
                }

                return;
            }

            UpdateAttraction();
        }

        public void Initialize(PlayerLootCollector collector, int experienceAmount)
        {
            _collector = collector;
            _experienceAmount = Mathf.Max(1, experienceAmount);
            _moveSpeed = _initialSpeed;
            _bobOffset = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            _isAttracted = false;
            _isReleased = false;
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

            Destroy(gameObject);
        }

        private void UpdateIdle()
        {
            float offset = Mathf.Sin(Time.time * _bobSpeed + _bobOffset) *
                _bobHeight;
            if (_visualTransform != null)
            {
                _visualTransform.localPosition = Vector3.up * offset;
            }
        }

        private void UpdateAttraction()
        {
            _moveSpeed = Mathf.Min(
                _maximumSpeed,
                _moveSpeed + _acceleration * Time.deltaTime);
            if (_visualTransform != null)
            {
                _visualTransform.localPosition = Vector3.zero;
            }
            transform.position = Vector2.MoveTowards(
                transform.position,
                _collector.Position,
                _moveSpeed * Time.deltaTime);

            if (((Vector2)transform.position - _collector.Position).sqrMagnitude >
                _collectDistance * _collectDistance)
            {
                return;
            }

            _collector.CollectExperience(_experienceAmount);
            Release();
        }

        private void OnValidate()
        {
            _initialSpeed = Mathf.Max(0.1f, _initialSpeed);
            _acceleration = Mathf.Max(0f, _acceleration);
            _maximumSpeed = Mathf.Max(_initialSpeed, _maximumSpeed);
            _collectDistance = Mathf.Max(0.01f, _collectDistance);
            _bobHeight = Mathf.Max(0f, _bobHeight);
            _bobSpeed = Mathf.Max(0f, _bobSpeed);
        }
    }
}
