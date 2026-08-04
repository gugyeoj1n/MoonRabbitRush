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
        ProjectileCrescentBoomerang,
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
    }
    public static class PoolingManager
    {
        private static Dictionary<PoolType, ObjectPool<GameObject>> poolDictionary = new Dictionary<PoolType, ObjectPool<GameObject>>();

        public static void RegisterPool(PoolType key, Func<GameObject> createFunc, int defaultCapacity = 10, int maxSize = 100)
        {
            var pool = new ObjectPool<GameObject>(
                    createFunc,        // 생성
                    OnGetObject,       // 가져올 때
                    OnReleaseObject,   // 반납할 때
                    OnDestroyObject,   // 삭제될 때
                    true,              // Collection Check
                    defaultCapacity,   // 기본 생성 개수
                    maxSize            // 최대 개수
                );

            if (poolDictionary.ContainsKey(key))
            {
                Debug.LogWarning($"Pool '{key}' already exists.");
                return;
            }
            poolDictionary[key] = pool;
        }

        public static void UnregisterPool(PoolType key)
        {
            if (poolDictionary.TryGetValue(key, out var pool))
            {
                pool.Clear();
                poolDictionary.Remove(key);
            }
            else
            {
                Debug.LogWarning($"Pool of type {key} does not exist.");
            }
        }

        public static bool IsRegistered(PoolType key)
        {
            return poolDictionary.ContainsKey(key);
        }

        public static void GetObject(PoolType key, out GameObject obj)
        {
            if (poolDictionary.TryGetValue(key, out var pool))
            {
                obj = pool.Get();
            }
            else
            {
                obj = null;
                Debug.LogWarning($"Pool of type {key} does not exist.");
            }
        }

        private static void OnGetObject(GameObject obj)
        {
            obj.SetActive(true);
        }

        private static void OnReleaseObject(GameObject obj)
        {
            obj.SetActive(false);
        }

        private static void OnDestroyObject(GameObject obj)
        {
            UnityEngine.Object.Destroy(obj);
        }

        public static void Release(PoolType key, GameObject obj)
        {
            if (poolDictionary.TryGetValue(key, out var pool))
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
            foreach (var pool in poolDictionary.Values)
                pool.Clear();

            poolDictionary.Clear();
            Debug.Log("Clearing all object pools on application quit.");
        }
    }
}
