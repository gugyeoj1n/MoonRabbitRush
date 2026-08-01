using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MoonRabbitRush.Combat;
using UnityEngine;

namespace MoonRabbitRush.Enemies.Bosses
{
    public sealed class BossMissileBarragePattern : BossAttackPattern
    {
        [SerializeField] private FallingAreaProjectile _projectilePrefab;
        [SerializeField, Min(1)] private int _missileCount = 5;
        [SerializeField, Min(0f)] private float _impactRadius = 0.85f;
        [SerializeField, Min(0f)] private float _fallHeight = 7f;
        [SerializeField, Min(0.05f)] private float _telegraphDuration = 0.85f;
        [SerializeField, Min(0f)] private float _spawnInterval = 0.14f;
        [SerializeField, Min(0f)] private float _randomOffsetRadius = 2.2f;

        [Header("Telegraph")]
        [SerializeField] private Sprite _outlineSprite;
        [SerializeField] private Sprite _fillSprite;
        [SerializeField, Range(0.1f, 1f)] private float _verticalScale = 0.72f;
        [SerializeField] private Color _outlineColor =
            new Color32(255, 83, 83, 204);
        [SerializeField] private Color _fillColor =
            new Color32(255, 129, 129, 115);

        public override async UniTask ExecuteAsync(
            CancellationToken cancellationToken)
        {
            if (_projectilePrefab == null)
            {
                return;
            }

            for (int index = 0; index < _missileCount; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Vector2 offset = index == 0
                    ? Vector2.zero
                    : UnityEngine.Random.insideUnitCircle * _randomOffsetRadius;
                Vector2 impactPosition = (Vector2)Target.position + offset;

                var telegraphObject =
                    new GameObject("Boss Missile Telegraph");
                CircleTelegraphView telegraph =
                    telegraphObject.AddComponent<CircleTelegraphView>();
                telegraph.Initialize(
                    impactPosition,
                    _impactRadius,
                    _telegraphDuration,
                    _outlineSprite,
                    _fillSprite,
                    _outlineColor,
                    _fillColor,
                    _verticalScale);

                FallingAreaProjectile projectile = Instantiate(
                    _projectilePrefab,
                    impactPosition,
                    Quaternion.identity);
                projectile.Launch(
                    impactPosition,
                    _fallHeight,
                    _impactRadius,
                    _telegraphDuration,
                    Stats.AttackDamage,
                    DamageTarget,
                    gameObject);

                if (_spawnInterval > 0f)
                {
                    await UniTask.Delay(
                        TimeSpan.FromSeconds(_spawnInterval),
                        DelayType.DeltaTime,
                        PlayerLoopTiming.Update,
                        cancellationToken);
                }
            }

            await UniTask.Delay(
                TimeSpan.FromSeconds(_telegraphDuration),
                DelayType.DeltaTime,
                PlayerLoopTiming.Update,
                cancellationToken);
        }

        private void OnValidate()
        {
            _missileCount = Mathf.Max(1, _missileCount);
            _impactRadius = Mathf.Max(0f, _impactRadius);
            _fallHeight = Mathf.Max(0f, _fallHeight);
            _telegraphDuration = Mathf.Max(0.05f, _telegraphDuration);
            _spawnInterval = Mathf.Max(0f, _spawnInterval);
            _randomOffsetRadius = Mathf.Max(0f, _randomOffsetRadius);
            _verticalScale = Mathf.Clamp(_verticalScale, 0.1f, 1f);
        }
    }
}
