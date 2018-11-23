using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateGoBack : FSMState<EnemyAI>
{
    [SerializeField] float delayToGoIdle = 0.5f;
    [SerializeField] float stoppingDistanceBeforeSpawnPosition = 5.0f;

    private void OnEnable()
    {
        agent.navMeshAgent.SetDestination(agent.spawnPosition);

        agent.navMeshAgent.speed        = agent.goBackSpeed;
        agent.minimumTimeTreshold       = 0.0f;
        agent.canInvestigateDisturbance = true;

        StartCoroutine(Work());
    }

    private void OnDisable()
    {
        agent.canInvestigateDisturbance = false;
        StopAllCoroutines();
    }

    private IEnumerator Work()
    {
        yield return new WaitForSeconds(delayToGoIdle);

        while(enabled)
        {
            if (!agent.isPlayerVisible && CloseToSpawnPosition())
                if (agent.canAgentPatrol) {
                    agent.fsm.ChangeState<EnemyStatePatrol>();
                }
                else agent.fsm.ChangeState<EnemyStateIdle>();

            yield return null;
        }
    }

    private bool CloseToSpawnPosition()
    {
        return (agent.spawnPosition - transform.position).sqrMagnitude 
            < stoppingDistanceBeforeSpawnPosition * stoppingDistanceBeforeSpawnPosition;
    }
}
