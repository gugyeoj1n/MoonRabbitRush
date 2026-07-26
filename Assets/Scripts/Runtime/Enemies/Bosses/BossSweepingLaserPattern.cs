using System.Collections;
using MoonRabbitRush.Combat;
using UnityEngine;

namespace MoonRabbitRush.Enemies.Bosses
{
    public sealed class BossSweepingLaserPattern : BossAttackPattern
    {
        [SerializeField, Min(0.05f)] private float _telegraphDuration = 0.8f;
        [SerializeField, Min(0.05f)] private float _sweepDuration = 1.3f;
        [SerializeField, Range(1f, 180f)] private float _sweepAngle = 120f;
        [SerializeField, Min(0.1f)] private float _beamLength = 12f;
        [SerializeField, Min(0.05f)] private float _telegraphWidth = 0.28f;
        [SerializeField, Min(0.05f)] private float _beamWidth = 0.55f;
        [SerializeField, Min(0.05f)] private float _damageInterval = 0.25f;
        [SerializeField] private Color _telegraphColor =
            new Color32(255, 83, 83, 130);
        [SerializeField] private Color _beamColor =
            new Color32(255, 60, 60, 245);

        private LineTelegraphView _activeLine;

        public override IEnumerator Execute()
        {
            Vector2 origin = transform.position;
            Vector2 targetDirection =
                ((Vector2)Target.position - origin).normalized;
            float centerAngle = Mathf.Atan2(
                targetDirection.y,
                targetDirection.x) * Mathf.Rad2Deg;
            float sweepDirection = Random.value < 0.5f ? -1f : 1f;
            float startAngle =
                centerAngle - _sweepAngle * 0.5f * sweepDirection;
            float endAngle =
                centerAngle + _sweepAngle * 0.5f * sweepDirection;

            _activeLine = CreateLine(
                "Boss Laser Telegraph",
                startAngle,
                _telegraphWidth,
                _telegraphColor);

            yield return new WaitForSeconds(_telegraphDuration);

            _activeLine.SetColor(_beamColor);
            float elapsed = 0f;
            float nextDamageTime = 0f;

            while (elapsed < _sweepDuration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / _sweepDuration);
                float angle = Mathf.Lerp(startAngle, endAngle, progress);
                Vector2 direction = DirectionFromAngle(angle);
                origin = transform.position;
                _activeLine.SetDirection(origin, direction);

                if (elapsed >= nextDamageTime)
                {
                    nextDamageTime = elapsed + _damageInterval;
                    TryDamageTarget(origin, direction);
                }

                yield return null;
            }

            _activeLine.Release();
            _activeLine = null;
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
            float angle,
            float width,
            Color color)
        {
            var lineObject = new GameObject(objectName);
            lineObject.AddComponent<LineRenderer>();
            LineTelegraphView line =
                lineObject.AddComponent<LineTelegraphView>();
            line.Initialize(
                transform.position,
                DirectionFromAngle(angle),
                _beamLength,
                width,
                color);
            return line;
        }

        private void TryDamageTarget(Vector2 origin, Vector2 direction)
        {
            if (!TargetDamageable.IsAlive)
            {
                return;
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

            if (Vector2.Distance(targetPosition, closestPoint) <=
                _beamWidth * 0.5f)
            {
                TargetDamageable.TakeDamage(
                    new DamageInfo(
                        Stats.AttackDamage,
                        closestPoint,
                        gameObject));
            }
        }

        private static Vector2 DirectionFromAngle(float angle)
        {
            float radians = angle * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }

        private void OnValidate()
        {
            _telegraphDuration = Mathf.Max(0.05f, _telegraphDuration);
            _sweepDuration = Mathf.Max(0.05f, _sweepDuration);
            _sweepAngle = Mathf.Clamp(_sweepAngle, 1f, 180f);
            _beamLength = Mathf.Max(0.1f, _beamLength);
            _telegraphWidth = Mathf.Max(0.05f, _telegraphWidth);
            _beamWidth = Mathf.Max(0.05f, _beamWidth);
            _damageInterval = Mathf.Max(0.05f, _damageInterval);
        }
    }
}
