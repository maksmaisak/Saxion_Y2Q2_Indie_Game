using System;
using UnityEngine;

public class StatsTracker : PersistentSingleton<StatsTracker>,
    IEventReceiver<OnLevelStarted>,
    IEventReceiver<OnEnemyDied>
{
    [Serializable]
    public class ScoreBonuses
    {
        [SerializeField] int enemyKillRegular  = 50;
        [SerializeField] int enemyKillHeadshot = 100;

        public int GetScoreFor(OnEnemyDied message)
        {
            return message.wasHeadshot ? enemyKillHeadshot : enemyKillRegular;
        }
    }

    [SerializeField] ScoreBonuses scoreBonuses;

    public ScoreBonuses getScoreBonuses => scoreBonuses;
    public int score { get; private set; }
    public int numKills { get; private set; }
    public int numHeadshots { get; private set; }

    public void On(OnEnemyDied message)
    {
        score += GetScoreFor(message);

        numKills += 1;
        if (message.wasHeadshot) numHeadshots += 1;
    }

    public int GetScoreFor(OnEnemyDied message) => scoreBonuses.GetScoreFor(message);
    public void On(OnLevelStarted message)
    {
        score = 0;
        numKills = 0;
        numHeadshots = 0;
    }
}