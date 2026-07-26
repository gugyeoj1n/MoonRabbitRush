using UnityEngine;

namespace MoonRabbitRush.Weapons
{
    public abstract class WeaponBehaviour : MonoBehaviour
    {
        public WeaponData Data { get; private set; }
        public int Level { get; private set; }
        protected Transform Owner { get; private set; }
        protected WeaponLevelStats Stats { get; private set; }
        public bool IsMaxLevel => Data != null && Level >= Data.MaxLevel;

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

        public bool TryActivateActiveSkill()
        {
            return IsMaxLevel && OnActivateActiveSkill();
        }

        protected virtual void OnInitialized()
        {
        }

        protected virtual void OnLevelChanged()
        {
        }

        protected virtual bool OnActivateActiveSkill()
        {
            return false;
        }
    }
}
