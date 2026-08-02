using UnityEngine;

namespace MoonRabbitRush.Combat
{
    public sealed class LineTelegraphView : MonoBehaviour
    {
        private const int ChargeSortingOrder = 12;
        private const int BeamSortingOrder = 13;

        private SpriteRenderer _chargeRenderer;
        private SpriteRenderer _beamRenderer;
        private Sprite[] _beamFrames;
        private float _beamFrameRate;
        private float _length;
        private float _width;
        private float _beamElapsed;
        private bool _isBeamActive;

        private void Awake()
        {
            CreateRenderersIfNeeded();
        }

        private void Update()
        {
            if (!_isBeamActive || _beamFrames == null || _beamFrames.Length == 0)
            {
                return;
            }

            _beamElapsed += Time.deltaTime;
            int frameIndex = Mathf.FloorToInt(_beamElapsed * _beamFrameRate);
            frameIndex %= _beamFrames.Length;
            _beamRenderer.sprite = _beamFrames[frameIndex];
        }

        public void InitializeCharge(
            Vector2 origin,
            float diameter,
            Sprite[] chargeFrames,
            Color color)
        {
            CreateRenderersIfNeeded();
            transform.position = origin;
            transform.rotation = Quaternion.identity;
            _length = 0f;
            _width = 0f;
            _beamElapsed = 0f;
            _isBeamActive = false;
            _beamRenderer.enabled = false;

            _chargeRenderer.enabled = chargeFrames != null && chargeFrames.Length > 0;
            _chargeRenderer.color = color;
            _chargeRenderer.drawMode = SpriteDrawMode.Simple;
            _chargeRenderer.sprite = _chargeRenderer.enabled ? chargeFrames[0] : null;

            if (_chargeRenderer.sprite == null)
            {
                return;
            }

            float spriteWidth = Mathf.Max(0.01f, _chargeRenderer.sprite.bounds.size.x);
            float scale = Mathf.Max(0.01f, diameter) / spriteWidth;
            _chargeRenderer.transform.localScale = new Vector3(scale, scale, 1f);
        }

        public void SetChargeProgress(Sprite[] chargeFrames, float progress)
        {
            if (!_chargeRenderer.enabled ||
                chargeFrames == null ||
                chargeFrames.Length == 0)
            {
                return;
            }

            int frameIndex = Mathf.Clamp(
                Mathf.FloorToInt(Mathf.Clamp01(progress) * chargeFrames.Length),
                0,
                chargeFrames.Length - 1);
            _chargeRenderer.sprite = chargeFrames[frameIndex];
        }

        public void StartBeam(
            float length,
            float width,
            Sprite[] beamFrames,
            Color color,
            float beamFrameRate)
        {
            CreateRenderersIfNeeded();
            _chargeRenderer.enabled = false;

            _length = Mathf.Max(0.01f, length);
            _width = Mathf.Max(0.01f, width);
            _beamFrames = beamFrames;
            _beamFrameRate = Mathf.Max(1f, beamFrameRate);
            _beamElapsed = 0f;
            _isBeamActive = beamFrames != null && beamFrames.Length > 0;

            _beamRenderer.enabled = _isBeamActive;
            _beamRenderer.color = color;
            _beamRenderer.drawMode = SpriteDrawMode.Sliced;
            _beamRenderer.size = new Vector2(_length, _width);
            _beamRenderer.sprite = _isBeamActive ? beamFrames[0] : null;
        }

        public void SetDirection(Vector2 origin, Vector2 direction)
        {
            Vector2 normalizedDirection = direction.sqrMagnitude > 0f
                ? direction.normalized
                : Vector2.right;
            float angle = Mathf.Atan2(normalizedDirection.y, normalizedDirection.x) *
                          Mathf.Rad2Deg;

            transform.position = origin + normalizedDirection * (_length * 0.5f);
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            if (_beamRenderer.enabled)
            {
                _beamRenderer.size = new Vector2(_length, _width);
            }
        }

        public void Release()
        {
            Destroy(gameObject);
        }

        private void CreateRenderersIfNeeded()
        {
            if (_chargeRenderer == null)
            {
                var chargeObject = new GameObject("Charge");
                chargeObject.transform.SetParent(transform, false);
                _chargeRenderer = chargeObject.AddComponent<SpriteRenderer>();
                _chargeRenderer.sortingOrder = ChargeSortingOrder;
            }

            if (_beamRenderer == null)
            {
                var beamObject = new GameObject("Beam");
                beamObject.transform.SetParent(transform, false);
                _beamRenderer = beamObject.AddComponent<SpriteRenderer>();
                _beamRenderer.sortingOrder = BeamSortingOrder;
                _beamRenderer.enabled = false;
            }
        }
    }
}
