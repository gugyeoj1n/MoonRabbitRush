namespace MoonRabbitRush.Enemies
{
    public interface IEnemy
    {
        bool IsActive { get; }
        void Activate();
        void Deactivate();
    }
}
