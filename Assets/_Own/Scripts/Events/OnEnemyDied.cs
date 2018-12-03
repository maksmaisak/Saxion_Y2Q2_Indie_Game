
public class OnEnemyDied : BroadcastEvent<OnEnemyDied>
{
    public readonly bool wasHeadshot;

    public OnEnemyDied(bool wasHeadshot)
    {
        this.wasHeadshot = wasHeadshot;
    }
}