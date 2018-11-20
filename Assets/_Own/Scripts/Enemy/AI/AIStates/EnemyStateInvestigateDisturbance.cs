using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

public class EnemyStateInvestigateDisturbance : FSMState<EnemyAI>
{
    [SerializeField] float minDelay = 0.1f;
    [SerializeField] float maxDelay = 1f;
    [SerializeField] float stoppingDistance = 5f;
    
    void OnEnable() => StartCoroutine(Work());
    void OnDisable() => StopAllCoroutines();

    IEnumerator Work()
    {
        yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));
        
        while (enabled)
        {
            Assert.IsTrue(agent.lastHeardDisturbancePositions.HasValue);
            agent.navMeshAgent.SetDestination(agent.lastHeardDisturbancePositions.Value);
           
            if (!agent.navMeshAgent.pathPending && agent.navMeshAgent.remainingDistance < Mathf.Max(stoppingDistance, agent.navMeshAgent.stoppingDistance))
            {
                agent.navMeshAgent.SetDestination(transform.position);
                agent.fsm.ChangeState<EnemyStateLookAround>();
            }
            
            yield return null;
        }
    }
}