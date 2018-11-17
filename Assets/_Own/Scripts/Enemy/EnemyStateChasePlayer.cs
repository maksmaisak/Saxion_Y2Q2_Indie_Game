using UnityEngine;

public class EnemyStateChasePlayer : FSMState<Enemy>
{
    [SerializeField] float stoppingDistanceBeforeLastPlayerPosition = 2f;
    
    void Update()
    {
        agent.navMeshAgent.SetDestination(agent.lastKnownPlayerPosition);

        if (!agent.isPlayerVisible && CloseToLastKnownPlayerLocation())
        {
            agent.fsm.ChangeState<EnemyStateWander>();
        }
    }

    bool CloseToLastKnownPlayerLocation()
    {
        return (transform.position - agent.lastKnownPlayerPosition).sqrMagnitude <
               stoppingDistanceBeforeLastPlayerPosition * stoppingDistanceBeforeLastPlayerPosition;
    }
}