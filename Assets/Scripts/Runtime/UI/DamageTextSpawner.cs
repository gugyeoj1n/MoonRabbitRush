using MoonRabbitRush.Combat;
using UnityEngine;

namespace MoonRabbitRush.UI
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class DamageTextSpawner : MonoBehaviour
    {
        [SerializeField] private DamageTextView _damageTextPrefab;
        [SerializeField] private Camera _worldCamera;
        [SerializeField] private Vector2 _horizontalOffset = new(-22f, 22f);
        [SerializeField] private Vector2 _verticalOffset = new(8f, 30f);
        [SerializeField] private Color32 _playerDamageColor = new(255, 90, 90, 255);

        private RectTransform _container;
        private Canvas _canvas;

        private void Awake()
        {
            _container = (RectTransform)transform;
            _canvas = GetComponentInParent<Canvas>();

            if (_worldCamera == null)
            {
                _worldCamera = Camera.main;
            }
        }

        private void OnEnable()
        {
            DamageFeedbackEvents.DamageApplied += Spawn;
        }

        private void OnDisable()
        {
            DamageFeedbackEvents.DamageApplied -= Spawn;
        }

        private void Spawn(float amount, Vector3 worldPosition, bool isPlayer)
        {
            if (_damageTextPrefab == null || _worldCamera == null || _canvas == null)
            {
                return;
            }

            Vector3 screenPosition = _worldCamera.WorldToScreenPoint(worldPosition);

            if (screenPosition.z < 0f)
            {
                return;
            }

            Camera uiCamera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : _canvas.worldCamera;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _container,
                    screenPosition,
                    uiCamera,
                    out Vector2 localPosition))
            {
                return;
            }

            localPosition += new Vector2(
                Random.Range(_horizontalOffset.x, _horizontalOffset.y),
                Random.Range(_verticalOffset.x, _verticalOffset.y));

            DamageTextView view = Instantiate(_damageTextPrefab, _container);
            view.Initialize(
                amount,
                localPosition,
                isPlayer ? _playerDamageColor : (Color32?)null);
        }

        private void OnValidate()
        {
            SortRange(ref _horizontalOffset);
            SortRange(ref _verticalOffset);
        }

        private static void SortRange(ref Vector2 range)
        {
            if (range.x > range.y)
            {
                (range.x, range.y) = (range.y, range.x);
            }
        }
    }
}
