using MoonRabbitRush.Defense;
using UnityEngine;

namespace MoonRabbitRush.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class BaseDirectionIndicator : MonoBehaviour
    {
        [SerializeField] private RectTransform _iconRoot;
        [SerializeField] private RectTransform _directionArrow;
        [SerializeField] private Transform _baseTarget;
        [SerializeField] private Camera _worldCamera;
        [SerializeField, Min(0f)] private float _edgePadding = 24f;
        [SerializeField, Min(0f)] private float _arrowOffset = 30f;

        private readonly Plane[] _frustumPlanes = new Plane[6];

        private RectTransform _boundaryRect;
        private CanvasGroup _canvasGroup;
        private Renderer _baseRenderer;

        private void Awake()
        {
            _iconRoot ??= transform as RectTransform;
            _boundaryRect = _iconRoot != null
                ? _iconRoot.parent as RectTransform
                : null;
            _canvasGroup = GetComponent<CanvasGroup>();
            ResolveReferences();
            SetVisible(false);
        }

        private void LateUpdate()
        {
            ResolveReferences();

            if (_iconRoot == null ||
                _boundaryRect == null ||
                _directionArrow == null ||
                _baseTarget == null ||
                _worldCamera == null)
            {
                SetVisible(false);
                return;
            }

            if (IsBaseVisible())
            {
                SetVisible(false);
                return;
            }

            Vector3 screenPosition =
                _worldCamera.WorldToScreenPoint(_baseTarget.position);
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _boundaryRect,
                    screenPosition,
                    null,
                    out Vector2 targetLocalPosition))
            {
                SetVisible(false);
                return;
            }

            Vector2 direction =
                targetLocalPosition - _boundaryRect.rect.center;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                SetVisible(false);
                return;
            }

            Vector2 normalizedDirection = direction.normalized;
            _iconRoot.anchoredPosition =
                CalculateEdgePosition(direction);
            _directionArrow.anchoredPosition =
                normalizedDirection * _arrowOffset;

            float angle = Mathf.Atan2(
                normalizedDirection.y,
                normalizedDirection.x) * Mathf.Rad2Deg;
            _directionArrow.localRotation =
                Quaternion.Euler(0f, 0f, angle - 90f);

            SetVisible(true);
        }

        private void ResolveReferences()
        {
            if (_baseTarget == null)
            {
                MoonBaseHealth baseHealth =
                    FindAnyObjectByType<MoonBaseHealth>();
                _baseTarget = baseHealth != null
                    ? baseHealth.transform
                    : null;
            }

            if (_baseRenderer == null && _baseTarget != null)
            {
                _baseRenderer =
                    _baseTarget.GetComponentInChildren<Renderer>();
            }

            if (_worldCamera == null)
            {
                _worldCamera =
                    ManagerRoot.Instance?.CameraMaanger?.MainCamera;
                _worldCamera ??= Camera.main;
            }
        }

        private bool IsBaseVisible()
        {
            if (_baseRenderer == null)
            {
                Vector3 viewportPosition =
                    _worldCamera.WorldToViewportPoint(_baseTarget.position);
                return viewportPosition.z > 0f &&
                       viewportPosition.x is >= 0f and <= 1f &&
                       viewportPosition.y is >= 0f and <= 1f;
            }

            GeometryUtility.CalculateFrustumPlanes(
                _worldCamera,
                _frustumPlanes);
            return GeometryUtility.TestPlanesAABB(
                _frustumPlanes,
                _baseRenderer.bounds);
        }

        private Vector2 CalculateEdgePosition(Vector2 direction)
        {
            Rect boundary = _boundaryRect.rect;
            Vector2 iconSize = _iconRoot.rect.size;
            float horizontalLimit = Mathf.Max(
                0f,
                boundary.width * 0.5f -
                iconSize.x * 0.5f -
                _edgePadding);
            float verticalLimit = Mathf.Max(
                0f,
                boundary.height * 0.5f -
                iconSize.y * 0.5f -
                _edgePadding);

            float horizontalScale = Mathf.Abs(direction.x) > Mathf.Epsilon
                ? horizontalLimit / Mathf.Abs(direction.x)
                : float.PositiveInfinity;
            float verticalScale = Mathf.Abs(direction.y) > Mathf.Epsilon
                ? verticalLimit / Mathf.Abs(direction.y)
                : float.PositiveInfinity;
            float edgeScale = Mathf.Min(horizontalScale, verticalScale);

            return direction * edgeScale;
        }

        private void SetVisible(bool isVisible)
        {
            if (_canvasGroup == null)
            {
                return;
            }

            _canvasGroup.alpha = isVisible ? 1f : 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
    }
}
