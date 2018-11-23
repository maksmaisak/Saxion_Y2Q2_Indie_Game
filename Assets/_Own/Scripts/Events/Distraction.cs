using UnityEngine;

public class Distraction : BroadcastEvent<Distraction>
{
    public readonly Vector3 position;
 
    public Distraction(Vector3 position)
    {
        this.position = position;
    }
}