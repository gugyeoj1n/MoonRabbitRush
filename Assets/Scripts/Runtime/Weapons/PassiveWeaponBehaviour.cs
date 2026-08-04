using UnityEngine;

namespace MoonRabbitRush.Weapons
{
    public sealed class PassiveWeaponBehaviour : WeaponBehaviour
    {
        protected override void OnLevelChanged()
        {
            if (Data == null || Modifiers == null ||
                !Data.TryGetPassiveLevelStats(Level, out PassiveWeaponLevelStats stats))
            {
                return;
            }

            Modifiers.Set(Data, stats);
        }
    }
}
