using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoonRabbitRush.Enemies
{
    public static class EnemyRegistry
    {
        private static readonly HashSet<EnemyActor> ActiveEnemies = new();

        public static int ActiveCount => ActiveEnemies.Count;
        public static event Action<int> ActiveCountChanged;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Clear()
        {
            ActiveEnemies.Clear();
            ActiveCountChanged = null;
        }

        public static void Register(EnemyActor enemy)
        {
            if (enemy != null)
            {
                if (ActiveEnemies.Add(enemy))
                {
                    ActiveCountChanged?.Invoke(ActiveEnemies.Count);
                }
            }
        }

        public static void Unregister(EnemyActor enemy)
        {
            if (enemy != null)
            {
                if (ActiveEnemies.Remove(enemy))
                {
                    ActiveCountChanged?.Invoke(ActiveEnemies.Count);
                }
            }
        }

        public static EnemyHealth FindClosest(Vector2 position, float range)
        {
            float maxSqrDistance = range * range;
            float closestSqrDistance = maxSqrDistance;
            EnemyHealth closest = null;

            foreach (EnemyActor enemy in ActiveEnemies)
            {
                if (enemy == null || !enemy.IsActive)
                {
                    continue;
                }

                float sqrDistance =
                    ((Vector2)enemy.transform.position - position).sqrMagnitude;

                if (sqrDistance > closestSqrDistance)
                {
                    continue;
                }

                closestSqrDistance = sqrDistance;
                closest = enemy.Health;
            }

            return closest;
        }

        public static void CollectInRange(
            Vector2 position,
            float range,
            List<EnemyHealth> results)
        {
            results.Clear();
            float maxSqrDistance = range * range;

            foreach (EnemyActor enemy in ActiveEnemies)
            {
                if (enemy == null || !enemy.IsActive)
                {
                    continue;
                }

                float sqrDistance =
                    ((Vector2)enemy.transform.position - position).sqrMagnitude;

                if (sqrDistance <= maxSqrDistance)
                {
                    results.Add(enemy.Health);
                }
            }
        }
    }
}
