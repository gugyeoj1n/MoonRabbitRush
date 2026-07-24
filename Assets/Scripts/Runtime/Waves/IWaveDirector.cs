namespace MoonRabbitRush.Waves
{
    public interface IWaveDirector
    {
        int CurrentWave { get; }
        bool IsRunning { get; }
        void StartNextWave();
        void Stop();
    }
}
