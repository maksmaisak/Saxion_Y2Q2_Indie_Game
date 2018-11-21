using UnityEngine;
using System.Collections;

public class EnemyStateChasePlayer : FSMState<EnemyAI>
{
    private void OnEnable()
    {
        agent.SetAIState(AIState.ChasePlayer);
        StartCoroutine(Work());
    }

    private void OnDisable() => StopAllCoroutines();

    private IEnumerator Work()
    {
        while (enabled)
        {
            agent.navMeshAgent.SetDestination(agent.lastKnownPlayerPosition);

            if (!agent.isPlayerVisible && agent.CloseToLastKnownPlayerLocation())
                agent.fsm.ChangeState<EnemyStateWander>();

            yield return null;
        }
    }
}