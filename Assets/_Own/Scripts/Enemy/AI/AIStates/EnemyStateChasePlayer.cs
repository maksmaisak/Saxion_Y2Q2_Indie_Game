using UnityEngine;

public class EnemyStateChasePlayer : FSMState<EnemyAI>
{
    [SerializeField] float stoppingDistanceBeforeLastPlayerPosition = 2f;
    [SerializeField] private bool isDestinationSet = false;
       
    void Update()
    {
        if (!isDestinationSet)
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