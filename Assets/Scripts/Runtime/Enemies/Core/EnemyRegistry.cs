using System.Collections.Generic;
using UnityEngine;

namespace MoonRabbitRush.Enemies
{
    public static class EnemyRegistry
    {
        private static readonly HashSet<EnemyActor> ActiveEnemies = new();

        public static void Register(EnemyActor enemy)
        {
            if (enemy != null)
            {
                ActiveEnemies.Add(enemy);
            }
        }

        public static void Unregister(EnemyActor enemy)
        {
            if (enemy != null)
            {
                ActiveEnemies.Remove(enemy);
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
    }
}
