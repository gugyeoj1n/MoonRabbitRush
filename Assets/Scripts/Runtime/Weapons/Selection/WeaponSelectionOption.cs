namespace MoonRabbitRush.Weapons.Selection
{
    public readonly struct WeaponSelectionOption
    {
        public WeaponSelectionOption(
            WeaponData weapon,
            int currentLevel,
            int targetLevel)
        {
            Weapon = weapon;
            CurrentLevel = currentLevel;
            TargetLevel = targetLevel;
        }

        public WeaponData Weapon { get; }
        public int CurrentLevel { get; }
        public int TargetLevel { get; }
        public bool IsNew => CurrentLevel == 0;
    }
}
