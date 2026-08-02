using UnityEngine;

namespace MoonRabbitRush.Score
{
    public static class ScoreStorage
    {
        private const string BestScoreKey = "score.best.value";
        private const string BestWaveKey = "score.best.wave";
        private const string BestSurvivalKey = "score.best.survival";
        private const string LastScoreKey = "score.last.value";
        private const string LastWaveKey = "score.last.wave";
        private const string LastSurvivalKey = "score.last.survival";
        private const string LastKillCountKey = "score.last.kills";
        private const string LastBossKillCountKey = "score.last.bossKills";

        public static ScoreRecord LoadBestRecord()
        {
            return new ScoreRecord(
                PlayerPrefs.GetInt(BestScoreKey, 0),
                PlayerPrefs.GetInt(BestWaveKey, 0),
                PlayerPrefs.GetFloat(BestSurvivalKey, 0f),
                0,
                0);
        }

        public static ScoreRecord LoadLastRecord()
        {
            return new ScoreRecord(
                PlayerPrefs.GetInt(LastScoreKey, 0),
                PlayerPrefs.GetInt(LastWaveKey, 0),
                PlayerPrefs.GetFloat(LastSurvivalKey, 0f),
                PlayerPrefs.GetInt(LastKillCountKey, 0),
                PlayerPrefs.GetInt(LastBossKillCountKey, 0));
        }

        public static ScoreRecord SaveRun(ScoreRecord currentRecord)
        {
            PlayerPrefs.SetInt(LastScoreKey, currentRecord.Score);
            PlayerPrefs.SetInt(LastWaveKey, currentRecord.Wave);
            PlayerPrefs.SetFloat(LastSurvivalKey, currentRecord.SurvivalSeconds);
            PlayerPrefs.SetInt(LastKillCountKey, currentRecord.KillCount);
            PlayerPrefs.SetInt(LastBossKillCountKey, currentRecord.BossKillCount);

            ScoreRecord bestRecord = LoadBestRecord();
            bool shouldUpdateBest =
                currentRecord.Score > bestRecord.Score ||
                (currentRecord.Score == bestRecord.Score &&
                 currentRecord.Wave > bestRecord.Wave) ||
                (currentRecord.Score == bestRecord.Score &&
                 currentRecord.Wave == bestRecord.Wave &&
                 currentRecord.SurvivalSeconds > bestRecord.SurvivalSeconds);

            if (shouldUpdateBest)
            {
                PlayerPrefs.SetInt(BestScoreKey, currentRecord.Score);
                PlayerPrefs.SetInt(BestWaveKey, currentRecord.Wave);
                PlayerPrefs.SetFloat(BestSurvivalKey, currentRecord.SurvivalSeconds);
                bestRecord = new ScoreRecord(
                    currentRecord.Score,
                    currentRecord.Wave,
                    currentRecord.SurvivalSeconds,
                    0,
                    0);
            }

            PlayerPrefs.Save();
            return bestRecord;
        }
    }
}
