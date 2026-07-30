using UnityEngine;

namespace MoonRabbitRush.Weapons
{
    public sealed class ShockDroneTrailView : MonoBehaviour
    {
        [Header("Shape")]
        [SerializeField, Min(0.01f)] private float _trailTime = 0.12f;
        [SerializeField, Min(0.005f)] private float _startWidth = 0.42f;
        [SerializeField, Min(0f)] private float _endWidth = 0.02f;
        [SerializeField, Min(0)] private int _cornerVertices = 4;
        [SerializeField, Min(0)] private int _endCapVertices = 4;

        [Header("Color")]
        [SerializeField] private Color _startColor =
            new(0.64f, 0.96f, 1f, 0.34f);
        [SerializeField] private Color _endColor =
            new(0.34f, 0.76f, 1f, 0f);

        [Header("Sorting")]
        [SerializeField] private string _sortingLayerName = "Default";
        [SerializeField] private int _sortingOrder = 10;

        private TrailRenderer _trailRenderer;
        private Material _runtimeMaterial;
        private bool _pendingEnableEmission;

        private void Awake()
        {
            _trailRenderer = GetComponent<TrailRenderer>();
            if (_trailRenderer == null)
            {
                _trailRenderer = gameObject.AddComponent<TrailRenderer>();
            }

            ConfigureTrailRenderer();
        }

        private void OnEnable()
        {
            _trailRenderer?.Clear();
            if (_trailRenderer != null)
            {
                _trailRenderer.emitting = false;
            }

            _pendingEnableEmission = true;
        }

        private void OnDisable()
        {
            _pendingEnableEmission = false;
            if (_trailRenderer != null)
            {
                _trailRenderer.emitting = false;
                _trailRenderer.Clear();
            }
        }

        private void LateUpdate()
        {
            if (!_pendingEnableEmission || _trailRenderer == null)
            {
                return;
            }

            _pendingEnableEmission = false;
            _trailRenderer.Clear();
            _trailRenderer.emitting = true;
        }

        private void OnDestroy()
        {
            if (_runtimeMaterial != null)
            {
                Destroy(_runtimeMaterial);
            }
        }

        private void ConfigureTrailRenderer()
        {
            _trailRenderer.time = Mathf.Max(0.01f, _trailTime);
            _trailRenderer.minVertexDistance = 0.03f;
            _trailRenderer.startWidth = Mathf.Max(0.005f, _startWidth);
            _trailRenderer.endWidth = Mathf.Max(0f, _endWidth);
            _trailRenderer.numCornerVertices = Mathf.Max(0, _cornerVertices);
            _trailRenderer.numCapVertices = Mathf.Max(0, _endCapVertices);
            _trailRenderer.shadowCastingMode =
                UnityEngine.Rendering.ShadowCastingMode.Off;
            _trailRenderer.receiveShadows = false;
            _trailRenderer.allowOcclusionWhenDynamic = false;
            _trailRenderer.alignment = LineAlignment.View;
            _trailRenderer.textureMode = LineTextureMode.Stretch;
            _trailRenderer.maskInteraction = SpriteMaskInteraction.None;
            _trailRenderer.autodestruct = false;
            _trailRenderer.sortingLayerName = _sortingLayerName;
            _trailRenderer.sortingOrder = _sortingOrder;
            _trailRenderer.startColor = _startColor;
            _trailRenderer.endColor = _endColor;

            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                _runtimeMaterial = new Material(shader);
                _trailRenderer.material = _runtimeMaterial;
            }
        }
    }
}
