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

        public int CurrentWave { get; private set; }
        public bool IsRunning => _waveRoutine != null;
        public float ElapsedTime { get; private set; }
        public float RemainingTime { get; private set; }

        public event Action<int> WaveStarted;
        public event Action<int> WaveCompleted;
        public event Action AllConfiguredWavesCompleted;

        private void Awake()
        {
            _spawner = GetComponent<EnemySpawner>();
        }

        private void OnEnable()
        {
            if (_startOnEnable)
            {
                StartNextWave();
            }
        }

        private void OnDisable()
        {
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

            ElapsedTime = 0f;
            RemainingTime = 0f;
        }

        private IEnumerator RunWave(WaveData wave)
        {
            CurrentWave = wave.WaveNumber;
            ElapsedTime = 0f;
            RemainingTime = wave.Duration;
            WaveStarted?.Invoke(CurrentWave);
            Debug.Log($"Wave {CurrentWave} started.", this);

            if (wave.SpawnEachEntryOnStart)
            {
                SpawnEachEntry(wave);
            }

            float spawnTimer = wave.SpawnInterval;

            while (ElapsedTime < wave.Duration)
            {
                yield return null;

                float deltaTime = Time.deltaTime;
                ElapsedTime += deltaTime;
                RemainingTime = Mathf.Max(0f, wave.Duration - ElapsedTime);
                spawnTimer -= deltaTime;

                if (spawnTimer > 0f)
                {
                    continue;
                }

                spawnTimer += wave.SpawnInterval;
                SpawnBatch(wave);
            }

            int completedWave = CurrentWave;
            RemainingTime = 0f;
            _waveRoutine = null;
            WaveCompleted?.Invoke(completedWave);
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
                    _spawner.Spawn(entry.Prefab);
                }
            }
        }

        private void SpawnBatch(WaveData wave)
        {
            int availableSlots = wave.MaxActiveEnemies - EnemyRegistry.ActiveCount;
            int spawnCount = Mathf.Min(wave.SpawnCount, availableSlots);

            for (int i = 0; i < spawnCount; i++)
            {
                EnemyActor prefab = wave.SelectEnemyPrefab();
                if (prefab != null)
                {
                    _spawner.Spawn(prefab);
                }
            }
        }
    }
}
