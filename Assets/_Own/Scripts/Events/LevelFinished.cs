
public class LevelFinished : BroadcastEvent<LevelFinished>
{
    public readonly int score;
    public readonly int kills;
    public readonly int headshots;

    public LevelFinished(int score, int kills, int headshots)
    {
        this.score = score;
        this.kills = kills;
        this.headshots = headshots;
    }
}