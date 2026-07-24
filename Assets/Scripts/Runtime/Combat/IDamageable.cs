namespace MoonRabbitRush.Combat
{
    public interface IDamageable
    {
        bool IsAlive { get; }
        void TakeDamage(in DamageInfo damage);
    }
}
