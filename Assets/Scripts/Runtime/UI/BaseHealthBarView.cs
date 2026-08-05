using MoonRabbitRush.Defense;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MoonRabbitRush.UI
{
    public sealed class BaseHealthBarView : MonoBehaviour
    {
        [SerializeField] private RectTransform _healthBarRoot;
        [SerializeField] private MoonBaseHealth _baseHealth;
        [SerializeField] private Camera _worldCamera;
        [SerializeField] private Vector3 _worldOffset =
            new(0f, 3f, 0f);

        private readonly Plane[] _frustumPlanes = new Plane[6];

        private RectTransform _positioningRect;
        private Slider _healthSlider;
        private TMP_Text _healthText;
        private Renderer _baseRenderer;

        private void Awake()
        {
            ResolveReferences();
            SetVisible(false);
        }

        private void OnEnable()
        {
            ResolveReferences();

            if (_baseHealth != null)
            {
                _baseHealth.HealthChanged -= HandleHealthChanged;
                _baseHealth.HealthChanged += HandleHealthChanged;
                HandleHealthChanged(
                    _baseHealth.CurrentHealth,
                    _baseHealth.MaxHealth);
            }
        }

        private void OnDisable()
        {
            if (_baseHealth != null)
            {
                _baseHealth.HealthChanged -= HandleHealthChanged;
            }
        }

        private void LateUpdate()
        {
            ResolveReferences();

            if (_healthBarRoot == null ||
                _positioningRect == null ||
                _baseHealth == null ||
                _worldCamera == null ||
                !IsBaseVisible())
            {
                SetVisible(false);
                return;
            }

            Vector3 worldPosition =
                _baseHealth.transform.position + _worldOffset;
            Vector3 screenPosition =
                _worldCamera.WorldToScreenPoint(worldPosition);
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _positioningRect,
                    screenPosition,
                    null,
                    out Vector2 localPosition))
            {
                SetVisible(false);
                return;
            }

            _healthBarRoot.anchoredPosition = localPosition;
            SetVisible(true);
        }

        private void ResolveReferences()
        {
            if (_healthBarRoot != null)
            {
                _positioningRect ??=
                    _healthBarRoot.parent as RectTransform;
                _healthSlider ??=
                    _healthBarRoot.GetComponent<Slider>();
                _healthText ??=
                    _healthBarRoot.GetComponentInChildren<TMP_Text>(true);
            }

            if (_baseHealth == null)
            {
                _baseHealth = FindAnyObjectByType<MoonBaseHealth>();
            }

            if (_baseRenderer == null && _baseHealth != null)
            {
                _baseRenderer =
                    _baseHealth.GetComponentInChildren<Renderer>();
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
                    _worldCamera.WorldToViewportPoint(
                        _baseHealth.transform.position);
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

        private void HandleHealthChanged(
            float currentHealth,
            float maxHealth)
        {
            if (_healthSlider == null)
            {
                return;
            }

            _healthSlider.minValue = 0f;
            _healthSlider.maxValue = Mathf.Max(1f, maxHealth);
            _healthSlider.SetValueWithoutNotify(
                Mathf.Clamp(currentHealth, 0f, maxHealth));

            if (_healthText != null)
            {
                float healthRatio = maxHealth > 0f
                    ? currentHealth / maxHealth
                    : 0f;
                _healthText.SetText(
                    "{0}%",
                    Mathf.RoundToInt(Mathf.Clamp01(healthRatio) * 100f));
            }
        }

        private void SetVisible(bool isVisible)
        {
            if (_healthBarRoot != null &&
                _healthBarRoot.gameObject.activeSelf != isVisible)
            {
                _healthBarRoot.gameObject.SetActive(isVisible);
            }
        }
    }
}
