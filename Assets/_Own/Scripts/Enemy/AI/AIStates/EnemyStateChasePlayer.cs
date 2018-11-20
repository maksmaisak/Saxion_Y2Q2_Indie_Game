using UnityEngine;

public class EnemyStateChasePlayer : FSMState<EnemyAI>
{
    [SerializeField] float stoppingDistanceBeforeLastPlayerPosition = 2f;
       
    void Update()
    {
        agent.navMeshAgent.SetDestination(agent.lastKnownPlayerPosition);

        if (!agent.isPlayerVisible && CloseToLastKnownPlayerLocation())
            agent.fsm.ChangeState<EnemyStateLookAround>();
    }

    bool CloseToLastKnownPlayerLocation()
    {
        return (transform.position - agent.lastKnownPlayerPosition).sqrMagnitude <
               stoppingDistanceBeforeLastPlayerPosition * stoppingDistanceBeforeLastPlayerPosition;
    }
}