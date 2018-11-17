using UnityEngine;

public class EnemyStateIdle : FSMState<Enemy>, IEventReceiver<Disturbance>
{
    public void On(Disturbance shot)
    {
        agent.lastHeardGunshotPosition = shot.position;
        
        if (enabled)
        {
            agent.fsm.ChangeState<EnemyStateInvestigateDisturbance>();
        }
    }
}