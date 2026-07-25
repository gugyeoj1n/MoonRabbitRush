using System.Collections.Generic;
using MoonRabbitRush.Enemies;
using UnityEngine;

namespace MoonRabbitRush.Waves
{
    [CreateAssetMenu(
        fileName = "SO_Wave_New",
        menuName = "Moon Rabbit Rush/Waves/Wave Data")]
    public sealed class WaveData : ScriptableObject
    {
        [SerializeField, Min(1)] private int _waveNumber = 1;
        [SerializeField, Min(0.1f)] private float _duration = 30f;
        [SerializeField, Min(0.05f)] private float _spawnInterval = 1.5f;
        [SerializeField, Min(1)] private int _spawnCount = 1;
        [SerializeField, Min(1)] private int _maxActiveEnemies = 30;
        [SerializeField] private bool _spawnEachEntryOnStart = true;
        [SerializeField] private EnemySpawnEntry[] _spawnEntries;

        public int WaveNumber => _waveNumber;
        public float Duration => _duration;
        public float SpawnInterval => _spawnInterval;
        public int SpawnCount => _spawnCount;
        public int MaxActiveEnemies => _maxActiveEnemies;
        public bool SpawnEachEntryOnStart => _spawnEachEntryOnStart;
        public IReadOnlyList<EnemySpawnEntry> SpawnEntries => _spawnEntries;

        public bool HasValidEntry
        {
            get
            {
                if (_spawnEntries == null)
                {
                    return false;
                }

                foreach (EnemySpawnEntry entry in _spawnEntries)
                {
                    if (entry != null && entry.IsValid)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public EnemyActor SelectEnemyPrefab()
        {
            float totalWeight = 0f;

            foreach (EnemySpawnEntry entry in _spawnEntries)
            {
                if (entry != null && entry.IsValid)
                {
                    totalWeight += entry.Weight;
                }
            }

            if (totalWeight <= 0f)
            {
                return null;
            }

            float selection = Random.Range(0f, totalWeight);
            EnemyActor fallback = null;

            foreach (EnemySpawnEntry entry in _spawnEntries)
            {
                if (entry == null || !entry.IsValid)
                {
                    continue;
                }

                fallback = entry.Prefab;
                selection -= entry.Weight;

                if (selection <= 0f)
                {
                    return entry.Prefab;
                }
            }

            return fallback;
        }
    }
}
