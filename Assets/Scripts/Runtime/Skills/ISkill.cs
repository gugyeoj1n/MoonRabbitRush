namespace MoonRabbitRush.Skills
{
    public interface ISkill
    {
        bool CanExecute { get; }
        void Execute();
    }
}
