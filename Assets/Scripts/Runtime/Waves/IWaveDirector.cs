namespace MoonRabbitRush.Waves
{
    public interface IWaveDirector
    {
        int CurrentWave { get; }
        bool IsRunning { get; }
        float ElapsedTime { get; }
        float RemainingTime { get; }
        void StartNextWave();
        void Stop();
    }
}
