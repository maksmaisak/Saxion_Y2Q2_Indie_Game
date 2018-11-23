using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

public class EnemyStateWander : FSMState<EnemyAI>
{
    [SerializeField] float duration = 10f;
    [SerializeField] float nextPositionDistance = 10f;
    [SerializeField] float minDistanceToObstacle = 3f;
    [SerializeField] float wanderTimeTreshold = 1f;

    void OnEnable()
    {
        if (Application.isPlaying)
            AIManager.instance.RegisterWanderer(agent);

        agent.navMeshAgent.speed  = agent.wanderSpeed;
        agent.minimumTimeTreshold = wanderTimeTreshold;
        
        this.Delay(duration, () => agent.fsm.ChangeState<EnemyStateGoBack>());
    }

    private void OnDisable()
    {
        if (Application.isPlaying)
            AIManager.instance.UnregisterWanderer(agent);
        
        StopAllCoroutines();
    } 

    void Update()
    {
        if (!agent.isPlayerVisible && agent.navMeshAgent.remainingDistance <= Mathf.Max(minDistanceToObstacle, agent.navMeshAgent.stoppingDistance)) {
            agent.navMeshAgent.destination = GetNextPosition();
        }
    }

    private Vector3 GetNextPosition()
    {
        Vector2 randomOnUnitCircle  = Random.insideUnitCircle.normalized * nextPositionDistance;
        Vector3 delta               = new Vector3(randomOnUnitCircle.x, 0f, randomOnUnitCircle.y);

        NavMeshHit hit;
        bool foundPosition = NavMesh.SamplePosition(transform.position + delta, out hit, nextPositionDistance, NavMesh.AllAreas);
        Assert.IsTrue(foundPosition);

        return hit.position;
    }
}