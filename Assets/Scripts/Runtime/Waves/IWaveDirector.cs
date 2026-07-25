namespace MoonRabbitRush.Waves
{
    public interface IWaveDirector
    {
        int CurrentWave { get; }
        bool IsRunning { get; }
        int SpawnedEnemyCount { get; }
        int RemainingEnemyCount { get; }
        void StartNextWave();
        void Stop();
    }
}
