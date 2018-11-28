public class EnemyDeath : BroadcastEvent<EnemyDeath>
{
    public readonly bool wasHeadshot;

    public EnemyDeath(bool wasHeadshot)
    {
        this.wasHeadshot = wasHeadshot;
    }
}