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

    private bool canChooseRandomPoint = false;
    
    void OnEnable()
    {
        agent.SetAIState(AIState.Wander);
        
        transform.DORotateQuaternion(Quaternion.LookRotation(agent.targetTransform.position - transform.position), 0.3f)
            .SetEase(Ease.Linear)
            .OnComplete(() => { canChooseRandomPoint = true; });
        
        this.Delay(duration, () => agent.fsm.ChangeState<EnemyStateGoBack>());
    }

    void Update()
    {
        if (canChooseRandomPoint && !agent.isPlayerVisible && agent.navMeshAgent.remainingDistance <= Mathf.Max(minDistanceToObstacle, agent.navMeshAgent.stoppingDistance)) {
            agent.navMeshAgent.destination = GetNextPosition();
        }
    }
    
    private Vector3 GetNextPosition()
    {
        Vector2 randomOnUnitCircle = Random.insideUnitCircle.normalized * nextPositionDistance;
        Vector3 delta = new Vector3(randomOnUnitCircle.x, 0f, randomOnUnitCircle.y);

        NavMeshHit hit;
        bool foundPosition = NavMesh.SamplePosition(transform.position + delta, out hit, nextPositionDistance, NavMesh.AllAreas);
        Assert.IsTrue(foundPosition);
        
        return hit.position;
    }
}