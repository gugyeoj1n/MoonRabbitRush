using System;
using UnityEngine;

namespace MoonRabbitRush.Enemies
{
    [Serializable]
    public sealed class EnemySpawnEntry
    {
        [SerializeField] private EnemyActor _prefab;
        [SerializeField, Min(0f)] private float _weight = 1f;

        public EnemyActor Prefab => _prefab;
        public float Weight => Mathf.Max(0f, _weight);
        public bool IsValid => _prefab != null && _weight > 0f;
    }
}
