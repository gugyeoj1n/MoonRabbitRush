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
            ResolveCanvas();
            ResolveWorldCamera();
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
            ResolveCanvas();
            ResolveWorldCamera();

            if (_damageTextPrefab == null || _worldCamera == null || _canvas == null)
            {
                return;
            }

            Camera uiCamera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : _canvas.worldCamera;
            Vector2 screenOffset = new(
                Random.Range(_horizontalOffset.x, _horizontalOffset.y),
                Random.Range(_verticalOffset.x, _verticalOffset.y));

            const PoolType poolType = PoolType.DamageText;
            if (!PoolingManager.IsRegistered(poolType))
            {
                PoolingManager.RegisterPool(
                    poolType,
                    () => Instantiate(_damageTextPrefab).gameObject,
                    defaultCapacity: 10,
                    maxSize: 100);
            }

            PoolingManager.GetObject(poolType, out GameObject textObject);
            if (textObject == null ||
                !textObject.TryGetComponent(out DamageTextView view))
            {
                return;
            }

            view.transform.SetParent(_container, false);
            view.Initialize(
                amount,
                worldPosition,
                screenOffset,
                _worldCamera,
                _container,
                uiCamera,
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

        private void ResolveCanvas()
        {
            if (_canvas == null)
            {
                _canvas = GetComponentInParent<Canvas>();
            }
        }

        private void ResolveWorldCamera()
        {
            if (_worldCamera != null)
            {
                return;
            }

            _worldCamera = Camera.main;
            if (_worldCamera != null)
            {
                return;
            }

            if (ManagerRoot.Instance?.CameraMaanger != null)
            {
                _worldCamera = ManagerRoot.Instance.CameraMaanger.MainCamera;
            }
        }
    }
}
