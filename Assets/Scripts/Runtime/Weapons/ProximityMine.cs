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
            Burrowing,
            Armed,
            Telegraphing
        }

        [Header("Burrow")]
        [SerializeField, Min(0.05f)] private float _burrowDuration = 0.3f;
        [SerializeField] private float _spinSpeed = 540f;

        [Header("Telegraph")]
        [SerializeField] private Sprite _outlineSprite;
        [SerializeField] private Sprite _fillSprite;
        [SerializeField, Range(0.1f, 1f)] private float _verticalScale = 0.72f;
        [SerializeField] private Color _outlineColor =
            new Color32(72, 210, 255, 255);
        [SerializeField] private Color _fillColor =
            new Color32(105, 225, 255, 140);

        private readonly List<EnemyHealth> _targets = new();
        private SpriteRenderer _spriteRenderer;
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
            switch (_state)
            {
                case MineState.Throwing:
                    UpdateThrow();
                    break;
                case MineState.Burrowing:
                    UpdateBurrow();
                    break;
                case MineState.Armed:
                    UpdateDetection();
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
                _state = MineState.Burrowing;
            }
        }

        private void UpdateBurrow()
        {
            _elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(_elapsed / _burrowDuration);
            transform.localScale = Vector3.Lerp(
                _initialScale,
                Vector3.zero,
                progress);

            Color color = _initialColor;
            color.a = 1f - progress;
            _spriteRenderer.color = color;

            if (progress >= 1f)
            {
                _elapsed = 0f;
                _state = MineState.Armed;
            }
        }

        private void UpdateDetection()
        {
            if (EnemyRegistry.FindClosest(transform.position, _stats.Range) != null)
            {
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

            _telegraph?.Release();
            _telegraph = null;
            Destroy(gameObject);
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
            _burrowDuration = Mathf.Max(0.05f, _burrowDuration);
            _verticalScale = Mathf.Clamp(_verticalScale, 0.1f, 1f);
        }
    }
}
