using UnityEngine;

public class Distraction : BroadcastEvent<Distraction>
{
    public readonly Vector3 position;
    public readonly float priority;
    public readonly float? enemyHearingRadius;
 
    public Distraction(Vector3 position, float priority, float? enemyHearingRadius = null)
    {
        this.position = position;
        this.priority = priority;
        this.enemyHearingRadius = enemyHearingRadius;
    }
}