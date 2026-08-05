using MoonRabbitRush.Player;
using UnityEngine;

namespace MoonRabbitRush.Items
{
    public sealed class WorldBuffSpawner : MonoBehaviour
    {
        [Header("Icons")]
        [SerializeField] private Sprite _healingCarrotIcon;
        [SerializeField] private Sprite _rabbitJetpackIcon;
        [SerializeField] private Sprite _moonlightShieldIcon;
        [SerializeField] private Sprite _shadowSprite;

        [Header("Spawn")]
        [SerializeField, Min(1f)] private float _minimumInterval = 12f;
        [SerializeField, Min(1f)] private float _maximumInterval = 18f;
        [SerializeField, Min(1f)] private float _minimumRadius = 7f;
        [SerializeField, Min(1f)] private float _maximumRadius = 11f;
        [SerializeField, Min(1)] private int _maximumActiveItems = 3;

        [Header("Effects")]
        [SerializeField, Min(1f)] private float _healingAmount = 35f;
        [SerializeField, Min(0f)] private float _speedBonus = 0.75f;
        [SerializeField, Min(0.1f)] private float _speedDuration = 6f;
        [SerializeField, Min(0.1f)] private float _shieldDuration = 5f;

        private PlayerHealth _playerHealth;
        private float _nextSpawnTime;

        private void Start()
        {
            _playerHealth = FindAnyObjectByType<PlayerHealth>();
            ScheduleNextSpawn(7f);
        }

        private void Update()
        {
            if (_playerHealth == null || !_playerHealth.IsAlive ||
                Time.time < _nextSpawnTime)
            {
                return;
            }

            if (FindObjectsByType<WorldBuffPickup>(
                    FindObjectsSortMode.None).Length < _maximumActiveItems)
            {
                SpawnRandomPickup();
            }

            ScheduleNextSpawn();
        }

        private void SpawnRandomPickup()
        {
            WorldBuffType buffType = (WorldBuffType)Random.Range(0, 3);
            Sprite icon = buffType switch
            {
                WorldBuffType.HealingCarrot => _healingCarrotIcon,
                WorldBuffType.RabbitJetpack => _rabbitJetpackIcon,
                _ => _moonlightShieldIcon,
            };

            Vector2 direction = Random.insideUnitCircle.normalized;
            float radius = Random.Range(_minimumRadius, _maximumRadius);
            Vector2 position = (Vector2)_playerHealth.transform.position +
                direction * radius;

            GameObject pickupObject = new($"Pickup_{buffType}");
            pickupObject.transform.position = position;
            WorldBuffPickup pickup = pickupObject.AddComponent<WorldBuffPickup>();
            pickup.Initialize(
                buffType,
                icon,
                _shadowSprite,
                _playerHealth,
                buffType == WorldBuffType.HealingCarrot
                    ? _healingAmount
                    : _speedBonus,
                buffType == WorldBuffType.RabbitJetpack
                    ? _speedDuration
                    : _shieldDuration);
        }

        private void ScheduleNextSpawn(float additionalDelay = 0f)
        {
            _nextSpawnTime = Time.time + additionalDelay +
                Random.Range(_minimumInterval, _maximumInterval);
        }

        private void OnValidate()
        {
            _maximumInterval = Mathf.Max(_minimumInterval, _maximumInterval);
            _maximumRadius = Mathf.Max(_minimumRadius, _maximumRadius);
        }
    }
}
