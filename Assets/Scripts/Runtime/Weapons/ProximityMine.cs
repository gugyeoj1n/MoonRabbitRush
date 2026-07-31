using System.Collections.Generic;
using MoonRabbitRush.Combat;
using MoonRabbitRush.Enemies;
using UnityEngine;

namespace MoonRabbitRush.Weapons
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class ProximityMine : MonoBehaviour
    {
        private enum MineState
        {
            Inactive,
            Throwing,
            Landing,
            Burrowing,
            Armed,
            Alerting,
            Telegraphing
        }

        [Header("Burrow")]
        [SerializeField] private Sprite _buriedSprite;
        [SerializeField, Min(0f)] private float _landingHoldDuration = 0.3f;
        [SerializeField, Min(0.05f)] private float _burrowDuration = 0.5f;
        [SerializeField] private float _spinSpeed = 540f;

        [Header("Alert")]
        [SerializeField] private Sprite _alertSprite;
        [SerializeField, Min(0.05f)] private float _alertDuration = 0.25f;
        [SerializeField] private Vector2 _alertLocalPosition = new(0f, 11f);
        [SerializeField, Min(0.01f)] private float _alertScale = 0.55f;

        [Header("Telegraph")]
        [SerializeField] private Sprite _outlineSprite;
        [SerializeField] private Sprite _fillSprite;
        [SerializeField, Range(0.1f, 1f)] private float _verticalScale = 0.72f;
        [SerializeField] private Color _outlineColor =
            new Color32(72, 210, 255, 204);
        [SerializeField] private Color _fillColor =
            new Color32(105, 225, 255, 115);

        [Header("Detection")]
        [SerializeField] private Sprite _detectionRangeSprite;
        [SerializeField] private Color _detectionRangeColor =
            new Color32(105, 225, 255, 92);
        [SerializeField] private int _detectionRangeSortingOrder = 8;
        [SerializeField] private float _detectionRangeRotationSpeed = -42f;

        [Header("Explosion")]
        [SerializeField] private TimedEffect _explosionEffectPrefab;

        private readonly List<EnemyHealth> _targets = new();
        private SpriteRenderer _spriteRenderer;
        private SpriteRenderer _alertRenderer;
        private SpriteRenderer _detectionRangeRenderer;
        private CircleTelegraphView _telegraph;
        private WeaponLevelStats _stats;
        private GameObject _source;
        private Vector2 _startPosition;
        private Vector2 _landingPosition;
        private Vector3 _initialScale;
        private Color _initialColor;
        private float _throwDuration;
        private float _arcHeight;
        private float _elapsed;
        private MineState _state;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _initialScale = transform.localScale;
            _initialColor = _spriteRenderer.color;
        }

        private void Update()
        {
            UpdateDetectionRangeVisual();

            switch (_state)
            {
                case MineState.Throwing:
                    UpdateThrow();
                    break;
                case MineState.Landing:
                    UpdateLanding();
                    break;
                case MineState.Burrowing:
                    UpdateBurrow();
                    break;
                case MineState.Armed:
                    UpdateDetection();
                    break;
                case MineState.Alerting:
                    UpdateAlert();
                    break;
                case MineState.Telegraphing:
                    UpdateTelegraph();
                    break;
            }
        }

        public void Launch(
            Vector2 landingPosition,
            float throwDuration,
            float arcHeight,
            in WeaponLevelStats stats,
            GameObject source)
        {
            _startPosition = transform.position;
            _landingPosition = landingPosition;
            _throwDuration = Mathf.Max(0.05f, throwDuration);
            _arcHeight = Mathf.Max(0f, arcHeight);
            _stats = stats;
            _source = source;
            _elapsed = 0f;
            _state = MineState.Throwing;
        }

        public bool TryForceDetonate()
        {
            if (_state != MineState.Armed &&
                _state != MineState.Alerting &&
                _state != MineState.Telegraphing)
            {
                return false;
            }

            if (_alertRenderer != null)
            {
                Destroy(_alertRenderer.gameObject);
                _alertRenderer = null;
            }

            if (_state != MineState.Telegraphing)
            {
                BeginTelegraph();
            }

            return true;
        }

        private void UpdateThrow()
        {
            _elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(_elapsed / _throwDuration);
            Vector2 position = Vector2.Lerp(
                _startPosition,
                _landingPosition,
                progress);
            position.y += 4f * _arcHeight * progress * (1f - progress);
            transform.position = position;
            transform.Rotate(0f, 0f, _spinSpeed * Time.deltaTime);

            if (progress >= 1f)
            {
                _elapsed = 0f;
                transform.position = _landingPosition;
                transform.rotation = Quaternion.identity;
                _state = MineState.Landing;
            }
        }

        private void UpdateLanding()
        {
            _elapsed += Time.deltaTime;

            if (_elapsed >= _landingHoldDuration)
            {
                _elapsed = 0f;
                _state = MineState.Burrowing;
            }
        }

        private void UpdateBurrow()
        {
            _elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(_elapsed / _burrowDuration);
            transform.localScale = Vector3.Lerp(
                _initialScale,
                _initialScale * 0.55f,
                progress);

            Color color = _initialColor;
            color.a = 1f - progress;
            _spriteRenderer.color = color;

            if (progress >= 1f)
            {
                _elapsed = 0f;
                _spriteRenderer.sprite = _buriedSprite;
                _spriteRenderer.color = _initialColor;
                transform.localScale = _initialScale;
                EnsureDetectionRangeVisual();
                _state = MineState.Armed;
            }
        }

        private void UpdateDetection()
        {
            if (EnemyRegistry.FindClosest(transform.position, _stats.Range) != null)
            {
                BeginAlert();
            }
        }

        private void BeginAlert()
        {
            var alertObject = new GameObject("Alert Exclamation");
            alertObject.transform.SetParent(transform, false);
            alertObject.transform.localPosition = _alertLocalPosition;
            alertObject.transform.localScale = Vector3.zero;
            _alertRenderer = alertObject.AddComponent<SpriteRenderer>();
            _alertRenderer.sprite = _alertSprite;
            _alertRenderer.color = Color.white;
            _alertRenderer.sortingOrder = _spriteRenderer.sortingOrder + 2;
            _elapsed = 0f;
            _state = MineState.Alerting;
        }

        private void UpdateAlert()
        {
            _elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(_elapsed / _alertDuration);
            float popScale = progress < 0.6f
                ? Mathf.Lerp(0f, 1.2f, progress / 0.6f)
                : Mathf.Lerp(1.2f, 1f, (progress - 0.6f) / 0.4f);
            _alertRenderer.transform.localScale =
                Vector3.one * (_alertScale * popScale);

            if (progress >= 1f)
            {
                Destroy(_alertRenderer.gameObject);
                _alertRenderer = null;
                BeginTelegraph();
            }
        }

        private void BeginTelegraph()
        {
            var telegraphObject = new GameObject("Player Mine Telegraph");
            _telegraph = telegraphObject.AddComponent<CircleTelegraphView>();
            _telegraph.Initialize(
                transform.position,
                _stats.AreaRadius,
                _stats.Duration,
                _outlineSprite,
                _fillSprite,
                _outlineColor,
                _fillColor,
                _verticalScale);
            _elapsed = 0f;
            _state = MineState.Telegraphing;
        }

        private void UpdateTelegraph()
        {
            _elapsed += Time.deltaTime;

            if (_elapsed >= _stats.Duration)
            {
                Explode();
            }
        }

        private void Explode()
        {
            ReleaseDetectionRangeVisual();

            EnemyRegistry.CollectInRange(
                transform.position,
                _stats.AreaRadius,
                _targets);

            foreach (EnemyHealth target in _targets)
            {
                if (target != null && target.IsAlive)
                {
                    target.TakeDamage(
                        new DamageInfo(_stats.Damage, transform.position, _source));
                }
            }

            SpawnExplosionEffect();
            _telegraph?.Release();
            _telegraph = null;
            Destroy(gameObject);
        }

        private void OnDisable()
        {
            ReleaseDetectionRangeVisual();
        }

        private void EnsureDetectionRangeVisual()
        {
            if (_detectionRangeSprite == null || _detectionRangeRenderer != null)
            {
                return;
            }

            var detectionRangeObject = new GameObject("Detection Range");
            _detectionRangeRenderer =
                detectionRangeObject.AddComponent<SpriteRenderer>();
            _detectionRangeRenderer.sprite = _detectionRangeSprite;
            _detectionRangeRenderer.color = _detectionRangeColor;
            _detectionRangeRenderer.sortingOrder = _detectionRangeSortingOrder;
            UpdateDetectionRangeTransform();
        }

        private void UpdateDetectionRangeVisual()
        {
            if (_detectionRangeRenderer == null)
            {
                return;
            }

            UpdateDetectionRangeTransform();
            _detectionRangeRenderer.transform.Rotate(
                0f,
                0f,
                _detectionRangeRotationSpeed * Time.deltaTime);
        }

        private void UpdateDetectionRangeTransform()
        {
            if (_detectionRangeRenderer == null)
            {
                return;
            }

            Transform detectionTransform = _detectionRangeRenderer.transform;
            detectionTransform.position = transform.position;

            float spriteWidth = Mathf.Max(
                0.01f,
                _detectionRangeSprite.bounds.size.x);
            float diameter = Mathf.Max(0.01f, _stats.Range * 2f);
            float scale = diameter / spriteWidth;
            detectionTransform.localScale = new Vector3(scale, scale, 1f);
        }

        private void ReleaseDetectionRangeVisual()
        {
            if (_detectionRangeRenderer == null)
            {
                return;
            }

            Destroy(_detectionRangeRenderer.gameObject);
            _detectionRangeRenderer = null;
        }

        private void SpawnExplosionEffect()
        {
            if (_explosionEffectPrefab != null)
            {
                Instantiate(
                    _explosionEffectPrefab,
                    transform.position,
                    Quaternion.identity);
            }
        }

        private void OnDestroy()
        {
            if (_telegraph != null)
            {
                _telegraph.Release();
            }
        }

        private void OnValidate()
        {
            _landingHoldDuration = Mathf.Max(0f, _landingHoldDuration);
            _burrowDuration = Mathf.Max(0.05f, _burrowDuration);
            _alertDuration = Mathf.Max(0.05f, _alertDuration);
            _alertScale = Mathf.Max(0.01f, _alertScale);
            _verticalScale = Mathf.Clamp(_verticalScale, 0.1f, 1f);
        }
    }
}
