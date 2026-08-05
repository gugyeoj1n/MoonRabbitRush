using MoonRabbitRush.Player;
using UnityEngine;

namespace MoonRabbitRush.Items
{
    public enum WorldBuffType
    {
        HealingCarrot,
        RabbitJetpack,
        MoonlightShield,
    }

    public sealed class WorldBuffPickup : MonoBehaviour
    {
        private const float CollectDistance = 1.2f;
        private const float BobHeight = 0.12f;
        private const float BobSpeed = 2.8f;

        private Transform _visual;
        private PlayerHealth _playerHealth;
        private WorldBuffType _buffType;
        private float _effectValue;
        private float _duration;
        private float _baseVisualY;
        private float _phase;

        public void Initialize(
            WorldBuffType buffType,
            Sprite icon,
            Sprite shadow,
            PlayerHealth playerHealth,
            float effectValue,
            float duration)
        {
            _buffType = buffType;
            _playerHealth = playerHealth;
            _effectValue = effectValue;
            _duration = duration;
            _phase = Random.Range(0f, Mathf.PI * 2f);

            CreateVisual(icon, shadow);
        }

        private void Update()
        {
            if (_playerHealth == null || !_playerHealth.IsAlive)
            {
                return;
            }

            float bobOffset = Mathf.Sin(Time.time * BobSpeed + _phase) * BobHeight;
            _visual.localPosition = new Vector3(0f, _baseVisualY + bobOffset, 0f);

            if (Vector2.SqrMagnitude(
                    (Vector2)_playerHealth.transform.position -
                    (Vector2)transform.position) <= CollectDistance * CollectDistance)
            {
                Collect();
            }
        }

        private void Collect()
        {
            switch (_buffType)
            {
                case WorldBuffType.HealingCarrot:
                    _playerHealth.Heal(_effectValue);
                    break;
                case WorldBuffType.RabbitJetpack:
                    PlayerTimedBuffs.GetOrAdd(_playerHealth.gameObject)
                        .ActivateSpeedBoost(_effectValue, _duration);
                    break;
                case WorldBuffType.MoonlightShield:
                    PlayerTimedBuffs.GetOrAdd(_playerHealth.gameObject)
                        .ActivateMoonlightShield(_duration);
                    break;
            }

            Destroy(gameObject);
        }

        private void CreateVisual(Sprite icon, Sprite shadow)
        {
            GameObject visualObject = new("Visual");
            _visual = visualObject.transform;
            _visual.SetParent(transform, false);
            _baseVisualY = 0.35f;
            _visual.localPosition = new Vector3(0f, _baseVisualY, 0f);
            _visual.localScale = Vector3.one * 0.65f;

            SpriteRenderer iconRenderer = visualObject.AddComponent<SpriteRenderer>();
            iconRenderer.sprite = icon;
            iconRenderer.sortingOrder = 7;

            GameObject shadowObject = new("Ground Shadow");
            shadowObject.transform.SetParent(transform, false);
            shadowObject.transform.localPosition = new Vector3(0f, -0.25f, 0f);
            shadowObject.transform.localScale = new Vector3(0.42f, 0.12f, 1f);
            SpriteRenderer shadowRenderer = shadowObject.AddComponent<SpriteRenderer>();
            shadowRenderer.sprite = shadow;
            shadowRenderer.color = new Color(0.11f, 0.12f, 0.15f, 0.75f);
            shadowRenderer.sortingOrder = 5;
        }
    }
}
