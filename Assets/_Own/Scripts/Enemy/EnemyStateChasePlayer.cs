using UnityEngine;

public class EnemyStateChasePlayer : FSMState<Enemy>
{    
    void Update()
    {
        agent.navMeshAgent.SetDestination(agent.lastKnownPlayerPosition);
    }
}