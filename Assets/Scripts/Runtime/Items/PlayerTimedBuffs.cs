using UnityEngine;

namespace MoonRabbitRush.Items
{
    public sealed class PlayerTimedBuffs : MonoBehaviour
    {
        private float _speedMultiplier = 1f;
        private float _speedBoostUntil;
        private TrailRenderer _speedTrail;
        private Material _trailMaterial;

        public float SpeedMultiplier => _speedMultiplier;

        public static PlayerTimedBuffs GetOrAdd(GameObject player)
        {
            PlayerTimedBuffs buffs = player.GetComponent<PlayerTimedBuffs>();
            return buffs != null ? buffs : player.AddComponent<PlayerTimedBuffs>();
        }

        private void Update()
        {
            if (_speedMultiplier > 1f && Time.time >= _speedBoostUntil)
            {
                _speedMultiplier = 1f;
                SetTrailEnabled(false);
            }
        }

        private void OnDisable()
        {
            _speedMultiplier = 1f;
            SetTrailEnabled(false);
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
