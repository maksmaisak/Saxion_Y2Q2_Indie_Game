using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

public class EnemyStateInvestigateDisturbance : FSMState<Enemy>
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
            Assert.IsTrue(agent.lastHeardGunshotPosition.HasValue);
            agent.navMeshAgent.SetDestination(agent.lastHeardGunshotPosition.Value);
            agent.navMeshAgent.isStopped = false;
            
            if (agent.navMeshAgent.remainingDistance < Mathf.Max(stoppingDistance, agent.navMeshAgent.stoppingDistance))
            {
                agent.navMeshAgent.isStopped = true;
                agent.fsm.ChangeState<EnemyStateWander>();
                yield break;
            }
            
            yield return null;
        }
    }
}