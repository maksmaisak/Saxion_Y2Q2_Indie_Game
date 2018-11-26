using UnityEngine;

public enum DistractionType
{
    AllyDeath = 1,
    Gunshot,
    Footstep,
    DistractionObjectHit, // TODO: Rename this
}

public class Distraction : BroadcastEvent<Distraction>
{
    public readonly Vector3 position;
    public readonly DistractionType distractionType;
 
    public Distraction(Vector3 position, DistractionType distractionType = DistractionType.AllyDeath)
    {
        this.position = position;
        this.distractionType = distractionType;
    }
}