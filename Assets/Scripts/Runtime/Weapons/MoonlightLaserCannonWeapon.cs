using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MoonRabbitRush.Combat;
using MoonRabbitRush.Enemies;
using UnityEngine;

namespace MoonRabbitRush.Weapons
{
    public sealed class MoonlightLaserCannonWeapon : WeaponBehaviour
    {
        [Header("Cannon Motion")]
        [SerializeField] private Transform _muzzle;
        [SerializeField] private Vector2 _floatingOffset = new(1.15f, 0.75f);
        [SerializeField, Min(0.01f)] private float _followSmoothTime = 0.12f;
        [SerializeField, Min(0f)] private float _bobDistance = 0.08f;
        [SerializeField, Min(0f)] private float _bobSpeed = 2.4f;
        [SerializeField, Min(1f)] private float _aimSpeed = 540f;
        [SerializeField, Min(0f)] private float _recoilDistance = 0.18f;
        [SerializeField, Min(0.01f)] private float _recoilRecoverySpeed = 8f;

        [Header("Normal Laser")]
        [SerializeField, Min(0.05f)] private float _telegraphDuration = 0.45f;
        [SerializeField, Min(0.05f)] private float _beamDuration = 0.2f;
        [SerializeField, Min(0.05f)] private float _beamWidth = 0.4f;
        [SerializeField, Min(0.1f)] private float _chargeDiameter = 0.75f;
        [SerializeField, Min(0.01f)] private float _additionalBeamInterval = 0.12f;

        [Header("Active Skill")]
        [SerializeField, Min(0.05f)] private float _activeChargeDuration = 0.8f;
        [SerializeField, Min(0.1f)] private float _activeBeamDuration = 3f;
        [SerializeField, Min(0.05f)] private float _activeBeamWidth = 1.1f;
        [SerializeField, Min(0.05f)] private float _activeDamageInterval = 0.18f;
        [SerializeField, Min(0f)] private float _activeDamageMultiplier = 0.6f;
        [SerializeField, Min(1f)] private float _activeAimSpeed = 70f;

        [Header("Visuals")]
        [SerializeField, Min(1f)] private float _beamFrameRate = 16f;
        [SerializeField] private Sprite[] _chargeFrames;
        [SerializeField] private Sprite[] _beamFrames;
        [SerializeField] private Color _telegraphColor =
            new(0.35f, 0.9f, 1f, 0.65f);
        [SerializeField] private Color _beamColor =
            new(0.45f, 0.95f, 1f, 0.9f);

        private readonly List<EnemyHealth> _enemyBuffer = new(64);
        private CancellationTokenSource _fireCts;
        private LineTelegraphView _activeLine;
        private Vector2 _followVelocity;
        private Vector2 _aimDirection = Vector2.right;
        private float _cooldownRemaining;
        private float _recoil;
        private int _fireSequence;
        private bool _isFiring;

        private void Update()
        {
            if (Owner == null || _muzzle == null)
            {
                return;
            }

            UpdateFloatingMotion();
            UpdateRecoil();

            if (_isFiring)
            {
                return;
            }

            EnemyHealth target = EnemyRegistry.FindClosest(
                Owner.position,
                Stats.Range);
            if (target != null)
            {
                RotateTowards(
                    (Vector2)target.transform.position - (Vector2)transform.position,
                    _aimSpeed);
            }

            _cooldownRemaining -= Time.deltaTime;
            if (_cooldownRemaining > 0f || target == null)
            {
                return;
            }

            StartNormalFire(target);
            _cooldownRemaining = Stats.Cooldown;
        }

        protected override void OnInitialized()
        {
            transform.localPosition = _floatingOffset;
        }

        protected override void OnLevelChanged()
        {
            _cooldownRemaining = Mathf.Min(_cooldownRemaining, Stats.Cooldown);
        }

        protected override bool OnActivateActiveSkill()
        {
            if (Owner == null || _muzzle == null ||
                EnemyRegistry.FindClosest(Owner.position, Stats.Range) == null)
            {
                return false;
            }

            CancelFire();
            _fireCts = new CancellationTokenSource();
            FireActiveLaserAsync(_fireSequence, _fireCts.Token).Forget();
            return true;
        }

        private void StartNormalFire(EnemyHealth target)
        {
            CancelFire();
            _fireCts = new CancellationTokenSource();
            FireNormalLaserAsync(
                target,
                _fireSequence,
                _fireCts.Token).Forget();
        }

        private async UniTaskVoid FireNormalLaserAsync(
            EnemyHealth target,
            int fireSequence,
            CancellationToken cancellationToken)
        {
            _isFiring = true;
            LineTelegraphView line = CreateLine("Moonlight Laser");
            _activeLine = line;
            if (line == null)
            {
                if (fireSequence == _fireSequence)
                {
                    _isFiring = false;
                }
                return;
            }

            try
            {
                float elapsed = 0f;
                while (elapsed < _telegraphDuration)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    elapsed += Time.deltaTime;

                    if (target != null && target.IsAlive)
                    {
                        RotateTowards(
                            (Vector2)target.transform.position -
                            (Vector2)transform.position,
                            _aimSpeed);
                    }

                    UpdateLineTransform();
                    line.SetChargeProgress(
                        _chargeFrames,
                        elapsed / _telegraphDuration);
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                }

                float length = Mathf.Max(0.1f, Stats.Range);
                float width = _beamWidth * Modifiers.SizeMultiplier;
                int beamCount = Mathf.Max(
                    1,
                    Stats.ProjectileCount + Modifiers.AdditionalWeaponCount);

                for (int beamIndex = 0; beamIndex < beamCount; beamIndex++)
                {
                    line.StartBeam(
                        length,
                        width,
                        _beamFrames,
                        _beamColor,
                        _beamFrameRate);
                    _recoil = _recoilDistance;
                    ApplyBeamDamage(
                        length,
                        width,
                        Stats.Damage * Modifiers.DamageMultiplier);

                    elapsed = 0f;
                    while (elapsed < _beamDuration)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        elapsed += Time.deltaTime;
                        UpdateLineTransform();
                        await UniTask.Yield(
                            PlayerLoopTiming.Update,
                            cancellationToken);
                    }

                    if (beamIndex >= beamCount - 1)
                    {
                        continue;
                    }

                    line.InitializeCharge(
                        _muzzle.position,
                        _chargeDiameter * Modifiers.SizeMultiplier,
                        _chargeFrames,
                        _telegraphColor);
                    elapsed = 0f;
                    while (elapsed < _additionalBeamInterval)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        elapsed += Time.deltaTime;
                        UpdateLineTransform();
                        line.SetChargeProgress(
                            _chargeFrames,
                            elapsed / _additionalBeamInterval);
                        await UniTask.Yield(
                            PlayerLoopTiming.Update,
                            cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                ReleaseLine(line);
                if (fireSequence == _fireSequence)
                {
                    _isFiring = false;
                }
            }
        }

        private async UniTaskVoid FireActiveLaserAsync(
            int fireSequence,
            CancellationToken cancellationToken)
        {
            _isFiring = true;
            _aimDirection = FindBestBeamDirection(Stats.Range);
            LineTelegraphView line = CreateLine("Moonlight Focus Laser");
            _activeLine = line;
            if (line == null)
            {
                if (fireSequence == _fireSequence)
                {
                    _isFiring = false;
                }
                return;
            }

            try
            {
                float elapsed = 0f;
                while (elapsed < _activeChargeDuration)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    elapsed += Time.deltaTime;
                    Vector2 desiredDirection = FindBestBeamDirection(Stats.Range);
                    RotateTowards(desiredDirection, _activeAimSpeed);
                    UpdateLineTransform();
                    line.SetChargeProgress(
                        _chargeFrames,
                        elapsed / _activeChargeDuration);
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                }

                float length = Mathf.Max(0.1f, Stats.Range * 1.25f);
                float width = _activeBeamWidth * Modifiers.SizeMultiplier;
                line.StartBeam(
                    length,
                    width,
                    _beamFrames,
                    _beamColor,
                    _beamFrameRate);
                _recoil = _recoilDistance;

                elapsed = 0f;
                float damageTimer = 0f;
                while (elapsed < _activeBeamDuration)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    elapsed += Time.deltaTime;
                    damageTimer -= Time.deltaTime;

                    Vector2 desiredDirection = FindBestBeamDirection(length);
                    RotateTowards(desiredDirection, _activeAimSpeed);
                    UpdateLineTransform();

                    if (damageTimer <= 0f)
                    {
                        ApplyBeamDamage(
                            length,
                            width,
                            Stats.Damage * Modifiers.DamageMultiplier *
                            _activeDamageMultiplier);
                        damageTimer = _activeDamageInterval;
                    }

                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                ReleaseLine(line);
                if (fireSequence == _fireSequence)
                {
                    _isFiring = false;
                }
            }
        }

        private LineTelegraphView CreateLine(string objectName)
        {
            LineTelegraphView line = LineTelegraphView.GetFromPool(objectName);
            if (line == null)
            {
                return null;
            }

            line.InitializeCharge(
                _muzzle.position,
                _chargeDiameter * Modifiers.SizeMultiplier,
                _chargeFrames,
                _telegraphColor);
            line.SetDirection(_muzzle.position, _aimDirection);
            return line;
        }

        private void UpdateFloatingMotion()
        {
            Vector2 bobOffset = Vector2.up *
                (Mathf.Sin(Time.time * _bobSpeed) * _bobDistance);
            Vector2 desiredPosition = _floatingOffset + bobOffset -
                                      _aimDirection * _recoil;
            transform.localPosition = Vector2.SmoothDamp(
                transform.localPosition,
                desiredPosition,
                ref _followVelocity,
                _followSmoothTime);
        }

        private void UpdateRecoil()
        {
            _recoil = Mathf.MoveTowards(
                _recoil,
                0f,
                _recoilRecoverySpeed * Time.deltaTime);
        }

        private void RotateTowards(Vector2 direction, float speed)
        {
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            float currentAngle = Mathf.Atan2(_aimDirection.y, _aimDirection.x) *
                                 Mathf.Rad2Deg;
            float targetAngle = Mathf.Atan2(direction.y, direction.x) *
                                Mathf.Rad2Deg;
            float angle = Mathf.MoveTowardsAngle(
                currentAngle,
                targetAngle,
                speed * Time.deltaTime);
            float radians = angle * Mathf.Deg2Rad;
            _aimDirection = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void UpdateLineTransform()
        {
            if (_activeLine != null)
            {
                _activeLine.SetDirection(_muzzle.position, _aimDirection);
            }
        }

        private Vector2 FindBestBeamDirection(float range)
        {
            EnemyRegistry.CollectInRange(Owner.position, range, _enemyBuffer);
            Vector2 bestDirection = _aimDirection;
            int bestScore = 0;

            foreach (EnemyHealth candidate in _enemyBuffer)
            {
                if (candidate == null || !candidate.IsAlive)
                {
                    continue;
                }

                Vector2 direction =
                    ((Vector2)candidate.transform.position - (Vector2)_muzzle.position)
                    .normalized;
                int score = CountEnemiesAlongBeam(
                    _muzzle.position,
                    direction,
                    range,
                    _activeBeamWidth);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestDirection = direction;
                }
            }

            return bestDirection.sqrMagnitude > Mathf.Epsilon
                ? bestDirection
                : Vector2.right;
        }

        private int CountEnemiesAlongBeam(
            Vector2 origin,
            Vector2 direction,
            float length,
            float width)
        {
            int count = 0;
            foreach (EnemyHealth enemy in _enemyBuffer)
            {
                if (enemy != null && enemy.IsAlive &&
                    DistanceToBeam(origin, direction, length, enemy) <=
                    width * 0.5f)
                {
                    count++;
                }
            }

            return count;
        }

        private void ApplyBeamDamage(float length, float width, float damage)
        {
            EnemyRegistry.CollectInRange(_muzzle.position, length, _enemyBuffer);
            Vector2 origin = _muzzle.position;

            foreach (EnemyHealth enemy in _enemyBuffer)
            {
                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                float distance = DistanceToBeam(
                    origin,
                    _aimDirection,
                    length,
                    enemy);
                if (distance > width * 0.5f)
                {
                    continue;
                }

                enemy.TakeDamage(new DamageInfo(
                    damage,
                    enemy.transform.position,
                    Owner.gameObject));
            }
        }

        private static float DistanceToBeam(
            Vector2 origin,
            Vector2 direction,
            float length,
            EnemyHealth enemy)
        {
            Vector2 end = origin + direction.normalized * length;
            Vector2 segment = end - origin;
            float segmentLengthSquared = segment.sqrMagnitude;
            Vector2 enemyPosition = enemy.transform.position;
            float projection = segmentLengthSquared > Mathf.Epsilon
                ? Vector2.Dot(enemyPosition - origin, segment) /
                  segmentLengthSquared
                : 0f;
            projection = Mathf.Clamp01(projection);
            Vector2 closestPoint = origin + segment * projection;
            Collider2D enemyCollider = enemy.GetComponent<Collider2D>();
            Vector2 enemyClosestPoint = enemyCollider != null
                ? enemyCollider.ClosestPoint(closestPoint)
                : enemyPosition;
            return Vector2.Distance(closestPoint, enemyClosestPoint);
        }

        private void ReleaseLine(LineTelegraphView line)
        {
            if (line == null || _activeLine != line)
            {
                return;
            }

            _activeLine = null;
            line.Release();
        }

        private void CancelFire()
        {
            _fireSequence++;

            if (_fireCts != null)
            {
                _fireCts.Cancel();
                _fireCts.Dispose();
                _fireCts = null;
            }

            ReleaseLine(_activeLine);
            _isFiring = false;
        }

        private void OnDisable()
        {
            CancelFire();
        }

        protected override void OnDestroy()
        {
            CancelFire();
            base.OnDestroy();
        }
    }
}
