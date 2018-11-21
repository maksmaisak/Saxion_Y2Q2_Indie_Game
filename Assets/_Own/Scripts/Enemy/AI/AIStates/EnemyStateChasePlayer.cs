using UnityEngine;
using System.Collections;
using DG.Tweening;
using Unity.Collections;

public class EnemyStateChasePlayer : FSMState<EnemyAI>
{  
    private Tweener currentTween;
    private bool isRotating = false;
    
    private void OnEnable()
    {
        agent.SetAIState(AIState.ChasePlayer);
        StartCoroutine(Work());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator Work()
    {
        while (enabled)
        {
            agent.navMeshAgent.SetDestination(agent.lastKnownPlayerPosition);

            transform.rotation = Quaternion.LookRotation(agent.targetTransform.position - transform.position);
            
            if (!agent.isPlayerVisible && agent.CloseToLastKnownPlayerLocation())              
                if (agent.GetTimeSinceLastPlayerSeen() > 2.0f)
                    agent.fsm.ChangeState<EnemyStateWander>();

            yield return null;
        }
    }
}