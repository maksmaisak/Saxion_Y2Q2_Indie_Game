using UnityEngine;

public class Distraction : BroadcastEvent<Distraction>
{
    public readonly Vector3 position;
    public readonly float distractionPriority;
 
    public Distraction(Vector3 position, float distractionPriority = 5.0f)
    {
        this.position = position;
        this.distractionPriority = distractionPriority;
    }
}