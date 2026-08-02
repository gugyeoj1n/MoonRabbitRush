using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MoonRabbitRush.Combat;
using UnityEngine;

namespace MoonRabbitRush.Enemies.Bosses
{
    public sealed class BossSweepingLaserPattern : BossAttackPattern
    {
        [SerializeField, Min(0.05f)] private float _telegraphDuration = 0.8f;
        [SerializeField, Min(0.05f)] private float _sweepDuration = 1.3f;
        [SerializeField, Range(1f, 180f)] private float _sweepAngle = 120f;
        [SerializeField, Range(0f, 90f)] private float _initialPlayerOffsetAngle = 15f;
        [SerializeField, Min(0.1f)] private float _beamLength = 12f;
        [SerializeField, Min(0.1f)] private float _chargeDiameter = 2.2f;
        [SerializeField, Min(0f)] private float _originForwardOffset = 0.8f;
        [SerializeField, Min(0.05f)] private float _beamWidth = 0.55f;
        [SerializeField, Min(0.05f)] private float _damageInterval = 0.25f;
        [SerializeField, Min(1f)] private float _beamFrameRate = 16f;
        [SerializeField] private Sprite[] _chargeFrames;
        [SerializeField] private Sprite[] _beamFrames;
        [SerializeField] private Color _telegraphColor = Color.white;
        [SerializeField] private Color _beamColor = Color.white;

        private LineTelegraphView _activeLine;
        private Collider2D _targetCollider;

        public override void Initialize(
            Transform target,
            EnemyStatsData stats)
        {
            base.Initialize(target, stats);
            _targetCollider = target.GetComponent<Collider2D>();
        }

        public override async UniTask ExecuteAsync(
            CancellationToken cancellationToken)
        {
            Vector2 origin = transform.position;
            Vector2 targetDirection =
                ((Vector2)Target.position - origin).normalized;
            if (targetDirection.sqrMagnitude <= Mathf.Epsilon)
            {
                targetDirection = Vector2.right;
            }

            float centerAngle = Mathf.Atan2(
                targetDirection.y,
                targetDirection.x) * Mathf.Rad2Deg;
            float sweepDirection =
                UnityEngine.Random.value < 0.5f ? -1f : 1f;
            float startAngle =
                centerAngle -
                (_sweepAngle * 0.5f + _initialPlayerOffsetAngle) * sweepDirection;
            float endAngle =
                startAngle + _sweepAngle * sweepDirection;

            _activeLine = CreateLine(
                "Boss Laser Telegraph",
                startAngle);
            if (_activeLine == null)
            {
                return;
            }

            try
            {
                float telegraphElapsed = 0f;
                Vector2 chargeOrigin = GetOrigin(targetDirection);
                _activeLine.SetDirection(chargeOrigin, targetDirection);

                while (telegraphElapsed < _telegraphDuration)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    telegraphElapsed += Time.deltaTime;
                    chargeOrigin = GetOrigin(targetDirection);
                    _activeLine.SetDirection(chargeOrigin, targetDirection);
                    _activeLine.SetChargeProgress(
                        _chargeFrames,
                        telegraphElapsed / _telegraphDuration);

                    await UniTask.Yield(
                        PlayerLoopTiming.Update,
                        cancellationToken);
                }

                _activeLine.StartBeam(
                    _beamLength,
                    _beamWidth,
                    _beamFrames,
                    _beamColor,
                    _beamFrameRate);
                float elapsed = 0f;
                float nextDamageTime = 0f;

                while (elapsed < _sweepDuration)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    elapsed += Time.deltaTime;
                    float progress = Mathf.Clamp01(elapsed / _sweepDuration);
                    float angle = Mathf.Lerp(startAngle, endAngle, progress);
                    Vector2 direction = DirectionFromAngle(angle);
                    origin = GetOrigin(direction);
                    _activeLine.SetDirection(origin, direction);

                    if (elapsed >= nextDamageTime &&
                        TryDamageTarget(origin, direction))
                    {
                        nextDamageTime = elapsed + _damageInterval;
                    }

                    await UniTask.Yield(
                        PlayerLoopTiming.Update,
                        cancellationToken);
                }
            }
            finally
            {
                if (_activeLine != null)
                {
                    _activeLine.Release();
                    _activeLine = null;
                }
            }
        }

        private void OnDisable()
        {
            if (_activeLine != null)
            {
                _activeLine.Release();
                _activeLine = null;
            }
        }

        private LineTelegraphView CreateLine(
            string objectName,
            float angle)
        {
            LineTelegraphView line =
                LineTelegraphView.GetFromPool(objectName);
            if (line == null)
            {
                return null;
            }

            line.InitializeCharge(
                GetOrigin(DirectionFromAngle(angle)),
                _chargeDiameter,
                _chargeFrames,
                _telegraphColor);
            line.SetDirection(
                GetOrigin(DirectionFromAngle(angle)),
                DirectionFromAngle(angle));
            return line;
        }

        private bool TryDamageTarget(Vector2 origin, Vector2 direction)
        {
            if (!TargetDamageable.IsAlive)
            {
                return false;
            }

            Vector2 targetPosition = Target.position;
            Vector2 end = origin + direction * _beamLength;
            Vector2 segment = end - origin;
            float segmentLengthSquared = segment.sqrMagnitude;
            float projection = segmentLengthSquared > 0f
                ? Vector2.Dot(targetPosition - origin, segment) /
                  segmentLengthSquared
                : 0f;
            projection = Mathf.Clamp01(projection);
            Vector2 closestPoint = origin + segment * projection;
            Vector2 targetClosestPoint = _targetCollider != null
                ? _targetCollider.ClosestPoint(closestPoint)
                : targetPosition;

            if (Vector2.Distance(targetClosestPoint, closestPoint) <=
                _beamWidth * 0.5f)
            {
                TargetDamageable.TakeDamage(
                    new DamageInfo(
                        Stats.AttackDamage,
                        closestPoint,
                        gameObject));
                return true;
            }

            return false;
        }

        private static Vector2 DirectionFromAngle(float angle)
        {
            float radians = angle * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }

        private Vector2 GetOrigin(Vector2 direction)
        {
            Vector2 normalizedDirection = direction.sqrMagnitude > Mathf.Epsilon
                ? direction.normalized
                : Vector2.right;
            return (Vector2)transform.position +
                   normalizedDirection * _originForwardOffset;
        }

        private void OnValidate()
        {
            _telegraphDuration = Mathf.Max(0.05f, _telegraphDuration);
            _sweepDuration = Mathf.Max(0.05f, _sweepDuration);
            _sweepAngle = Mathf.Clamp(_sweepAngle, 1f, 180f);
            _initialPlayerOffsetAngle = Mathf.Clamp(_initialPlayerOffsetAngle, 0f, 90f);
            _beamLength = Mathf.Max(0.1f, _beamLength);
            _chargeDiameter = Mathf.Max(0.1f, _chargeDiameter);
            _originForwardOffset = Mathf.Max(0f, _originForwardOffset);
            _beamWidth = Mathf.Max(0.05f, _beamWidth);
            _damageInterval = Mathf.Max(0.05f, _damageInterval);
            _beamFrameRate = Mathf.Max(1f, _beamFrameRate);
        }
    }
}
