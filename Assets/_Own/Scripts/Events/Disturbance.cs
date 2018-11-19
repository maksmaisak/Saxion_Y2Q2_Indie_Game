using UnityEngine;

public class Disturbance : BroadcastEvent<Disturbance>
{
    public readonly Vector3 position;
    
    public Disturbance(Vector3 position)
    {
        this.position = position;
    }
}