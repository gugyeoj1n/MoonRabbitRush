using UnityEngine;

namespace MoonRabbitRush.Enemies
{
    public abstract class EnemyBehaviour : MonoBehaviour
    {
        protected Transform Target { get; private set; }
        protected EnemyStatsData Stats { get; private set; }

        public virtual void Initialize(Transform target, EnemyStatsData stats)
        {
            Target = target;
            Stats = stats;
        }
    }
}
