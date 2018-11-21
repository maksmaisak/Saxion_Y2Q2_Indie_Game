using System.Collections;
using Cinemachine.Utility;
using UnityEngine;
using DG.Tweening;

public class EnemyStateDistracted : FSMState<EnemyAI>
{
    [SerializeField] private float secondsUntillChase = 2.0f;
    [SerializeField] private float secondsUntillMoveToTarget = 1.0f;
    [SerializeField] private float maxSecondsDistracted = 4.0f;

    private bool isMovingToTarget;
    private bool hasDestinationSet;
    private float secondsDistracted;

    private void OnEnable()
    {
        secondsDistracted       = 0;
        agent.SetAIState(AIState.Distracted);

        StartCoroutine(Work());
    }

    private void OnDisable() => StopAllCoroutines();

    IEnumerator Work()
    {
        while (enabled)
        {
            secondsDistracted += Time.deltaTime;

            if (agent.seenTimeDiff >= secondsUntillMoveToTarget)
            {
                if (!isMovingToTarget)
                {
                    isMovingToTarget = true;
                    transform.DORotateQuaternion(Quaternion.LookRotation(agent.lastKnownPlayerPosition - transform.position), 0.3f).SetEase(Ease.Linear);
                    StartCoroutine(InvestigatePlayer());
                }
            }

            if (agent.seenTimeDiff >= secondsUntillChase)
                agent.fsm.ChangeState<EnemyStateChasePlayer>();

            if (secondsDistracted >= maxSecondsDistracted)
                agent.fsm.ChangeState<EnemyStateIdle>();

            yield return null;
        }
    }

    IEnumerator InvestigatePlayer()
    {
        yield return new WaitForSeconds(0.8f);
        agent.fsm.ChangeState<EnemyStateInvestigatePlayer>();
    }
}
