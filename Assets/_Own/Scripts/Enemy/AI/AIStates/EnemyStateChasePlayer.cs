using UnityEngine;
using System.Collections;
using DG.Tweening;
using Unity.Collections;

public class EnemyStateChasePlayer : FSMState<EnemyAI>
{
    [SerializeField] float minimumChaseTimeTreshold = 2.0f;
    [SerializeField] float secondsToEvadeMode = 3.0f;
    [SerializeField] float faceTargetSpeed = 2.0f;

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

            if (!agent.isPlayerVisible)
                transform.rotation = Quaternion.RotateTowards(transform.rotation,
                    Quaternion.LookRotation(agent.targetTransform.position - transform.position),
                    Time.deltaTime * faceTargetSpeed);

            if (!agent.isPlayerVisible && agent.CloseToLastKnownPlayerLocation())
                if (agent.GetTimeSinceLastPlayerSeen() > minimumChaseTimeTreshold)
                    agent.fsm.ChangeState<EnemyStateWander>();

            yield return null;
        }
    }
}