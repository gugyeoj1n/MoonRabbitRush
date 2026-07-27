using UnityEngine;

namespace MoonRabbitRush.Combat
{
    [RequireComponent(typeof(LineRenderer))]
    public sealed class LineTelegraphView : MonoBehaviour
    {
        private LineRenderer _lineRenderer;
        private Material _runtimeMaterial;
        private float _length;

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _lineRenderer.useWorldSpace = true;
            _lineRenderer.positionCount = 2;
            _lineRenderer.numCapVertices = 4;
            _lineRenderer.sortingOrder = 6;

            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                _runtimeMaterial = new Material(shader);
                _lineRenderer.material = _runtimeMaterial;
            }
        }

        public void Initialize(
            Vector2 origin,
            Vector2 direction,
            float length,
            float width,
            Color color)
        {
            _length = Mathf.Max(0.01f, length);
            _lineRenderer.startWidth = Mathf.Max(0.01f, width);
            _lineRenderer.endWidth = Mathf.Max(0.01f, width);
            _lineRenderer.startColor = color;
            _lineRenderer.endColor = color;
            SetDirection(origin, direction);
        }

        public void SetDirection(Vector2 origin, Vector2 direction)
        {
            Vector2 normalizedDirection = direction.sqrMagnitude > 0f
                ? direction.normalized
                : Vector2.right;
            _lineRenderer.SetPosition(0, origin);
            _lineRenderer.SetPosition(
                1,
                origin + normalizedDirection * _length);
        }

        public void SetColor(Color color)
        {
            _lineRenderer.startColor = color;
            _lineRenderer.endColor = color;
        }

        public void Release()
        {
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (_runtimeMaterial != null)
            {
                Destroy(_runtimeMaterial);
            }
        }
    }
}
