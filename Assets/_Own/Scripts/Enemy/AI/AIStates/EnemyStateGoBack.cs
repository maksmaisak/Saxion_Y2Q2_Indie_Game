using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateGoBack : FSMState<EnemyAI>
{
    [SerializeField] float stoppingDistanceBeforeSpawnPosition = 5.0f;

    private void OnEnable()
    {
        agent.SetAIState(AIState.GoingBack);
        agent.navMeshAgent.SetDestination(agent.spawnPosition);
    }

    private IEnumerator Work()
    {
        yield return new WaitForSeconds(0.5f);

        while(enabled)
        {
            if (!agent.isPlayerVisible && CloseToSpawnPosition())
                agent.fsm.ChangeState<EnemyStateGoBack>();

            yield return null;
        }
    }

    private bool CloseToSpawnPosition()
    {
        return (agent.spawnPosition - transform.position).sqrMagnitude 
            < stoppingDistanceBeforeSpawnPosition * stoppingDistanceBeforeSpawnPosition;
    }
}
