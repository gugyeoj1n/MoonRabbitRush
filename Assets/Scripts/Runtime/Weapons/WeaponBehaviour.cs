using UnityEngine;

namespace MoonRabbitRush.Weapons
{
    public abstract class WeaponBehaviour : MonoBehaviour
    {
        public WeaponData Data { get; private set; }
        public int Level { get; private set; }
        protected Transform Owner { get; private set; }
        protected WeaponLevelStats Stats { get; private set; }

        public void Initialize(Transform owner, WeaponData data, int level)
        {
            Owner = owner;
            Data = data;
            SetLevel(level);
            OnInitialized();
        }

        public bool SetLevel(int level)
        {
            if (Data == null || !Data.TryGetLevelStats(level, out WeaponLevelStats stats))
            {
                return false;
            }

            Level = level;
            Stats = stats;
            OnLevelChanged();
            return true;
        }

        protected virtual void OnInitialized()
        {
        }

        protected virtual void OnLevelChanged()
        {
        }
    }
}
