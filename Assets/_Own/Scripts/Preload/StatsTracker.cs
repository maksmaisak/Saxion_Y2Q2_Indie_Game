using System;
using UnityEngine;

public class StatsTracker : Singleton<StatsTracker>,
    IEventReceiver<LevelStarted>,
    IEventReceiver<EnemyDied>
{
    [Serializable]
    class ScoreBonuses
    {
        [SerializeField] int enemyKillRegular  = 50;
        [SerializeField] int enemyKillHeadshot = 100;

        public int GetScoreFor(EnemyDied message)
        {
            return message.wasHeadshot ? enemyKillHeadshot : enemyKillRegular;
        }
    }

    [SerializeField] ScoreBonuses scoreBonuses;
    
    public int score { get; private set; }
    public int numKills { get; private set; }
    public int numHeadshots { get; private set; }
    
    public void On(EnemyDied message)
    {
        score += GetScoreFor(message);

        numKills += 1;
        if (message.wasHeadshot) numHeadshots += 1;
    }

    public int GetScoreFor(EnemyDied message) => scoreBonuses.GetScoreFor(message);
    public void On(LevelStarted message)
    {
        score = 0;
        numKills = 0;
        numHeadshots = 0;
    }
}