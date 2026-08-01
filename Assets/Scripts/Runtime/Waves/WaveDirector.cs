using System;
using System.Collections;
using MoonRabbitRush.Enemies;
using UnityEngine;

namespace MoonRabbitRush.Waves
{
    [RequireComponent(typeof(EnemySpawner))]
    public sealed class WaveDirector : MonoBehaviour, IWaveDirector
    {
        [SerializeField] private WaveData[] _waves;
        [SerializeField] private bool _startOnEnable = true;

        private EnemySpawner _spawner;
        private Coroutine _waveRoutine;
        private int _waveIndex = -1;
        private int _currentWaveEnemyCount;

        public int CurrentWave { get; private set; }
        public bool IsRunning => _waveRoutine != null;
        public int SpawnedEnemyCount { get; private set; }
        public int RemainingEnemyCount =>
            Mathf.Max(0, _currentWaveEnemyCount - SpawnedEnemyCount)
            + EnemyRegistry.ActiveCount;

        public event Action<int> WaveStarted;
        public event Action<int> WaveCompleted;
        public event Action<int> RemainingEnemyCountChanged;
        public event Action AllConfiguredWavesCompleted;

        private void Awake()
        {
            _spawner = GetComponent<EnemySpawner>();
        }

        private void OnEnable()
        {
            EnemyRegistry.ActiveCountChanged += HandleActiveEnemyCountChanged;

            if (_startOnEnable)
            {
                StartNextWave();
            }
        }

        private void OnDisable()
        {
            EnemyRegistry.ActiveCountChanged -= HandleActiveEnemyCountChanged;
            Stop();
        }

        public void StartNextWave()
        {
            if (IsRunning)
            {
                return;
            }

            int nextIndex = _waveIndex + 1;
            if (_waves == null || nextIndex >= _waves.Length)
            {
                AllConfiguredWavesCompleted?.Invoke();
                Debug.Log("All configured waves completed. Boss wave is not configured yet.", this);
                return;
            }

            WaveData wave = _waves[nextIndex];
            if (wave == null || !wave.HasValidEntry)
            {
                Debug.LogError($"Wave data at index {nextIndex} is invalid.", this);
                return;
            }

            _waveIndex = nextIndex;
            _waveRoutine = StartCoroutine(RunWave(wave));
        }

        public void Stop()
        {
            if (_waveRoutine != null)
            {
                StopCoroutine(_waveRoutine);
                _waveRoutine = null;
            }

            SpawnedEnemyCount = 0;
            _currentWaveEnemyCount = 0;
            RemainingEnemyCountChanged?.Invoke(0);
        }

        private IEnumerator RunWave(WaveData wave)
        {
            CurrentWave = wave.WaveNumber;
            SpawnedEnemyCount = 0;
            _currentWaveEnemyCount = wave.TotalEnemyCount;
            WaveStarted?.Invoke(CurrentWave);
            RemainingEnemyCountChanged?.Invoke(RemainingEnemyCount);
            Debug.Log($"Wave {CurrentWave} started.", this);

            if (wave.SpawnEachEntryOnStart)
            {
                SpawnEachEntry(wave);
            }

            while (SpawnedEnemyCount < wave.TotalEnemyCount)
            {
                yield return new WaitForSeconds(wave.SpawnInterval);
                SpawnBatch(wave);
            }

            yield return new WaitUntil(() => EnemyRegistry.ActiveCount == 0);

            int completedWave = CurrentWave;
            _waveRoutine = null;
            WaveCompleted?.Invoke(completedWave);
            RemainingEnemyCountChanged?.Invoke(0);
            Debug.Log($"Wave {completedWave} completed.", this);

            StartNextWave();
        }

        private void SpawnEachEntry(WaveData wave)
        {
            foreach (EnemySpawnEntry entry in wave.SpawnEntries)
            {
                if (EnemyRegistry.ActiveCount >= wave.MaxActiveEnemies)
                {
                    return;
                }

                if (entry != null && entry.IsValid)
                {
                    if (SpawnedEnemyCount >= wave.TotalEnemyCount)
                    {
                        return;
                    }

                    if (_spawner.Spawn(entry.Prefab) != null)
                    {
                        SpawnedEnemyCount++;
                        RemainingEnemyCountChanged?.Invoke(RemainingEnemyCount);
                    }
                }
            }
        }

        private void SpawnBatch(WaveData wave)
        {
            int availableSlots = wave.MaxActiveEnemies - EnemyRegistry.ActiveCount;
            int remainingSpawnCount = wave.TotalEnemyCount - SpawnedEnemyCount;
            int spawnCount = Mathf.Min(
                Mathf.Min(wave.SpawnCount, availableSlots),
                remainingSpawnCount);

            for (int i = 0; i < spawnCount; i++)
            {
                EnemyActor prefab = wave.SelectEnemyPrefab();
                if (prefab != null && _spawner.Spawn(prefab) != null)
                {
                    SpawnedEnemyCount++;
                    RemainingEnemyCountChanged?.Invoke(RemainingEnemyCount);
                }
            }
        }

        private void HandleActiveEnemyCountChanged(int _)
        {
            if (!enabled)
            {
                return;
            }

            RemainingEnemyCountChanged?.Invoke(RemainingEnemyCount);
        }
    }
}
