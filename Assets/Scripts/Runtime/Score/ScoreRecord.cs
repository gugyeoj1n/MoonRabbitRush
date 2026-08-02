namespace MoonRabbitRush.Score
{
    public readonly struct ScoreRecord
    {
        public ScoreRecord(
            int score,
            int wave,
            float survivalSeconds,
            int killCount,
            int bossKillCount)
        {
            Score = score;
            Wave = wave;
            SurvivalSeconds = survivalSeconds;
            KillCount = killCount;
            BossKillCount = bossKillCount;
        }

        public int Score { get; }
        public int Wave { get; }
        public float SurvivalSeconds { get; }
        public int KillCount { get; }
        public int BossKillCount { get; }
    }
}
