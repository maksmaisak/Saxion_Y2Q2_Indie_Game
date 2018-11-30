
public class EnemyDied : BroadcastEvent<EnemyDied>
{
    public readonly bool wasHeadshot;

    public EnemyDied(bool wasHeadshot)
    {
        this.wasHeadshot = wasHeadshot;
    }
}