using UnityEngine;
using System.Collections;
using DG.Tweening;
using Unity.Collections;

public class EnemyStateChasePlayer : FSMState<EnemyAI>
{
    [SerializeField] float minimumChaseTimeTreshold = 2.0f;
    [SerializeField] float secondsToEvadeMode = 3.0f;
    [SerializeField] float faceTargetSpeed = 10.0f;

    private void OnEnable()
    {
        agent.navMeshAgent.speed  = agent.chaseSpeed;
        agent.minimumTimeTreshold = minimumChaseTimeTreshold;
        StartCoroutine(Work());
    }

    private void OnDisable() => StopAllCoroutines();

    private IEnumerator Work()
    {
        while (enabled)
        {
            agent.navMeshAgent.SetDestination(agent.lastKnownPlayerPosition);

            if (agent.navMeshAgent.remainingDistance  < agent.navMeshAgent.stoppingDistance)
            {
                if (!agent.isPlayerVisible)
                    if (agent.GetTimeSinceLastPlayerSeen() > secondsToEvadeMode)
                        agent.fsm.ChangeState<EnemyStateWander>();

                if (agent.isPlayerVisible)
                {
                    Vector3 targetPosition   = agent.targetTransform.position;
                    targetPosition.y         = transform.position.y;
                    
                    transform.rotation = Quaternion.RotateTowards(transform.rotation,
                        Quaternion.LookRotation(targetPosition - transform.position), 
                        Time.deltaTime * faceTargetSpeed);
                }
            }

            yield return null;
        }
    }
}