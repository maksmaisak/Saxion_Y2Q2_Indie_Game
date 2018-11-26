using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;
using UnityEngine.SocialPlatforms;

public class EnemyStateWander : FSMState<EnemyAI>
{
    [SerializeField] float duration = 10f;
    [SerializeField] float nextPositionDistance = 10f;
    [SerializeField] float minDistanceToObstacle = 3f;
    [SerializeField] float wanderTimeTreshold = 1f;
    
    [Range(1, 100)]
    [SerializeField] int chanceToWanderInPlayerDirection = 30;

    void OnEnable()
    {
        agent.navMeshAgent.speed  = agent.wanderSpeed;
        agent.minimumAwarenessLevelThreshold = wanderTimeTreshold;

        this.Delay(duration, () => agent.fsm.ChangeState<EnemyStateGoBack>());
    }

    private void OnDisable()
    {
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
        if (chanceToWanderInPlayerDirection > Random.Range(0, 99)){
            Vector3 direction = (agent.targetTransform.position - agent.transform.position).normalized;
            randomOnUnitCircle = new Vector2(direction.x, direction.z) * nextPositionDistance;
        }

        Vector3 delta               = new Vector3(randomOnUnitCircle.x, 0f, randomOnUnitCircle.y);

        NavMeshHit hit;
        bool foundPosition = NavMesh.SamplePosition(transform.position + delta, out hit, nextPositionDistance, NavMesh.AllAreas);
        Assert.IsTrue(foundPosition);
        
        return hit.position;
    }
}