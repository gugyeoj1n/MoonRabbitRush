using UnityEngine;
using MoonRabbitRush.Player;

namespace MoonRabbitRush.Items
{
    public sealed class PlayerTimedBuffs : MonoBehaviour
    {
        [SerializeField] private Sprite _shieldSprite;

        private float _speedMultiplier = 1f;
        private float _speedBoostUntil;
        private float _shieldUntil;
        private TrailRenderer _speedTrail;
        private Material _trailMaterial;
        private SpriteRenderer _shieldRenderer;
        private PlayerHealth _health;

        public float SpeedMultiplier => _speedMultiplier;

        public static PlayerTimedBuffs GetOrAdd(GameObject player)
        {
            PlayerTimedBuffs buffs = player.GetComponent<PlayerTimedBuffs>();
            return buffs != null ? buffs : player.AddComponent<PlayerTimedBuffs>();
        }

        private void Awake()
        {
            _health = GetComponent<PlayerHealth>();
        }

        private void Update()
        {
            if (_speedMultiplier > 1f && Time.time >= _speedBoostUntil)
            {
                _speedMultiplier = 1f;
                SetTrailEnabled(false);
            }

            if (_shieldRenderer != null && _shieldRenderer.enabled)
            {
                if (Time.time >= _shieldUntil)
                {
                    _shieldRenderer.enabled = false;
                    return;
                }

                float pulse = 1f + Mathf.Sin(Time.time * 5f) * 0.04f;
                _shieldRenderer.transform.localScale =
                    new Vector3(2.5f, 2.5f, 1f) * pulse;
            }
        }

        private void OnDisable()
        {
            _speedMultiplier = 1f;
            SetTrailEnabled(false);
            if (_shieldRenderer != null)
            {
                _shieldRenderer.enabled = false;
            }
        }

        private void OnDestroy()
        {
            if (_trailMaterial != null)
            {
                Destroy(_trailMaterial);
            }
        }

        public void ActivateSpeedBoost(float speedBonus, float duration)
        {
            _speedMultiplier = 1f + Mathf.Max(0f, speedBonus);
            _speedBoostUntil = Time.time + Mathf.Max(0.1f, duration);
            EnsureSpeedTrail();
            _speedTrail.Clear();
            SetTrailEnabled(true);
        }

        public void ActivateMoonlightShield(float duration)
        {
            float safeDuration = Mathf.Max(0.1f, duration);
            _shieldUntil = Time.time + safeDuration;
            _health ??= GetComponent<PlayerHealth>();
            _health?.GrantInvincibility(safeDuration);
            EnsureShieldVisual();
            _shieldRenderer.enabled = true;
        }

        private void EnsureShieldVisual()
        {
            if (_shieldRenderer != null)
            {
                return;
            }

            GameObject shieldObject = new("Moonlight Shield Visual");
            shieldObject.transform.SetParent(transform, false);
            shieldObject.transform.localScale = new Vector3(2.5f, 2.5f, 1f);
            _shieldRenderer = shieldObject.AddComponent<SpriteRenderer>();
            _shieldRenderer.sprite = _shieldSprite;
            _shieldRenderer.color = new Color(0.38f, 0.9f, 1f, 0.25f);
            _shieldRenderer.sortingOrder = 12;
        }

        private void EnsureSpeedTrail()
        {
            if (_speedTrail != null)
            {
                return;
            }

            _speedTrail = gameObject.AddComponent<TrailRenderer>();
            _speedTrail.time = 0.22f;
            _speedTrail.minVertexDistance = 0.04f;
            _speedTrail.startWidth = 0.75f;
            _speedTrail.endWidth = 0.05f;
            _speedTrail.numCornerVertices = 4;
            _speedTrail.numCapVertices = 4;
            _speedTrail.alignment = LineAlignment.View;
            _speedTrail.textureMode = LineTextureMode.Stretch;
            _speedTrail.shadowCastingMode =
                UnityEngine.Rendering.ShadowCastingMode.Off;
            _speedTrail.receiveShadows = false;
            _speedTrail.sortingOrder = 8;
            _speedTrail.startColor = new Color(0.25f, 0.95f, 1f, 0.62f);
            _speedTrail.endColor = new Color(0.12f, 0.55f, 1f, 0f);

            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                _trailMaterial = new Material(shader);
                _speedTrail.material = _trailMaterial;
            }

            _speedTrail.emitting = false;
        }

        private void SetTrailEnabled(bool isEnabled)
        {
            if (_speedTrail == null)
            {
                return;
            }

            _speedTrail.emitting = isEnabled;
            if (!isEnabled)
            {
                _speedTrail.Clear();
            }
        }
    }
}
