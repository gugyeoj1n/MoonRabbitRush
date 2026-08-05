using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace MoonRabbitRush
{
    public enum PoolType
    {
        EnemyGloby,
        EnemyInkto,
        EnemyOrbitron,
        EnemyBossUfo,
        ExperienceCarrot,
        ProjectileCarrotMissile,
        ProjectileInktoInk,
        ProjectileOrbitronMissile,
        WeaponShockDrone,
        WeaponSpaceCarrotMine,
        EffectCarrotMissileImpact,
        EffectShockDroneContact,
        EffectSpaceCarrotMineExplosion,
        TelegraphCircle,
        TelegraphLine,
        DamageText,
        ProjectileCrescentBoomerang,
    }
    public static class PoolingManager
    {
        private static readonly Dictionary<PoolType, ObjectPool<GameObject>>
            PoolDictionary = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetPools()
        {
            PoolDictionary.Clear();
        }

        public static void RegisterPool(PoolType key, Func<GameObject> createFunc, int defaultCapacity = 10, int maxSize = 100)
        {
            if (PoolDictionary.ContainsKey(key))
            {
                Debug.LogWarning($"Pool '{key}' already exists.");
                return;
            }

            var pool = new ObjectPool<GameObject>(
                    createFunc,        // 생성
                    OnGetObject,       // 가져올 때
                    OnReleaseObject,   // 반납할 때
                    OnDestroyObject,   // 삭제될 때
                    true,              // Collection Check
                    defaultCapacity,   // 기본 생성 개수
                    maxSize            // 최대 개수
                );

            PoolDictionary[key] = pool;
        }

        public static void UnregisterPool(PoolType key)
        {
            if (PoolDictionary.TryGetValue(key, out var pool))
            {
                pool.Clear();
                PoolDictionary.Remove(key);
            }
            else
            {
                Debug.LogWarning($"Pool of type {key} does not exist.");
            }
        }

        public static bool IsRegistered(PoolType key)
        {
            return PoolDictionary.ContainsKey(key);
        }

        public static void GetObject(PoolType key, out GameObject obj)
        {
            if (PoolDictionary.TryGetValue(key, out var pool))
            {
                try
                {
                    obj = pool.Get();
                }
                catch (MissingReferenceException exception)
                {
                    Debug.LogWarning(
                        $"Discarding stale pool '{key}' after its scene owner was destroyed. " +
                        exception.Message);
                    PoolDictionary.Remove(key);
                    obj = null;
                }
            }
            else
            {
                obj = null;
                Debug.LogWarning($"Pool of type {key} does not exist.");
            }
        }

        private static void OnGetObject(GameObject obj)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }

        private static void OnReleaseObject(GameObject obj)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }

        private static void OnDestroyObject(GameObject obj)
        {
            if (obj == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(obj);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(obj);
            }
        }

        public static void Release(PoolType key, GameObject obj)
        {
            if (obj == null)
            {
                return;
            }

            if (PoolDictionary.TryGetValue(key, out var pool))
            {
                pool.Release(obj);
            }
            else
            {
                Debug.LogWarning($"Pool of type {key} does not exist.");
            }
        }
        public static void Clear()
        {
            foreach (var pool in PoolDictionary.Values)
                pool.Clear();

            PoolDictionary.Clear();
        }
    }
}
