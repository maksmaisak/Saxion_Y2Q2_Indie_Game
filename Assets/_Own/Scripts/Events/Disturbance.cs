using UnityEngine;

public class Disturbance : BroadcastEvent<Disturbance>
{
    public readonly GameObject gameObject;
 
    public Disturbance(GameObject gameObject)
    {
        this.gameObject = gameObject;
    }
}