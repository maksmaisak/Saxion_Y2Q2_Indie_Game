using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateInvestigatePlayer : FSMState<EnemyAI>
{
    [SerializeField] float secondsToChasePlayer = 2.0f;

    void OnEnable()
    {
        agent.SetAIState(AIState.InvestigatePlayer);

        StartCoroutine(Work());
    }

    void OnDisable() => StopAllCoroutines();

    IEnumerator Work()
    {
        while (enabled)
        {
            // With really low speed
            agent.navMeshAgent.SetDestination(agent.lastKnownPlayerPosition);

            if (agent.seenTimeDiff >= secondsToChasePlayer)
                agent.fsm.ChangeState<EnemyStateChasePlayer>();

            if (!agent.isPlayerVisible && agent.CloseToLastKnownPlayerLocation())
                agent.fsm.ChangeState<EnemyStateLookAround>();

            yield return null;
        }
    }
}
