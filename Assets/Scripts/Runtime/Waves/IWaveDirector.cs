namespace MoonRabbitRush.Waves
{
    public interface IWaveDirector
    {
        int CurrentWave { get; }
        int CurrentBossRound { get; }
        bool IsRunning { get; }
        int SpawnedEnemyCount { get; }
        int RemainingEnemyCount { get; }
        void StartNextWave();
        void CompleteBossEncounter();
        void Stop();
    }
}
