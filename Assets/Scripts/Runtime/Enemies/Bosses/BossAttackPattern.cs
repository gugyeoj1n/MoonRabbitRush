using System.Threading;
using Cysharp.Threading.Tasks;
using MoonRabbitRush.Combat;
using UnityEngine;

namespace MoonRabbitRush.Enemies.Bosses
{
    public abstract class BossAttackPattern : EnemyBehaviour
    {
        protected Component DamageTarget { get; private set; }
        protected IDamageable TargetDamageable { get; private set; }
        public bool IsReady =>
            Target != null && DamageTarget != null && TargetDamageable != null;

        public override void Initialize(
            Transform target,
            EnemyStatsData stats)
        {
            base.Initialize(target, stats);
            DamageTarget =
                target.GetComponent(typeof(IDamageable)) as Component;
            TargetDamageable = DamageTarget as IDamageable;

            if (TargetDamageable == null)
            {
                Debug.LogError(
                    "Boss attack target must implement IDamageable.",
                    this);
            }
        }

        public abstract UniTask ExecuteAsync(CancellationToken cancellationToken);
    }
}
