using UnityEngine;

public class OnInvestigateDeath : BroadcastEvent<OnInvestigateDeath>
{
    public readonly Vector3 position;
 
    public OnInvestigateDeath(Vector3 position)
    {
        this.position = position;
    }
}