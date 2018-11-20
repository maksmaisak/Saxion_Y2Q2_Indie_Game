using System.Collections;
using Cinemachine.Utility;
using UnityEngine;

public class EnemyStateDistracted : FSMState<EnemyAI>
{
    [SerializeField] private float secondsUntillChase = 2.0f;
    [SerializeField] private float secondsUntillMoveToTarget = 0.5f;

    private bool isMovingToTarget;
    private bool hasDestinationSet;
    
    private float seenTimeDiff;
    private float seenTimeMultiplier = 1.0f;

    private void OnEnable()
    {
        StartCoroutine(UpdateSeenTimeMultiplier());
        StartCoroutine(Work());
    }

    private void OnDisable() => StopAllCoroutines();

    IEnumerator Work()
    {
        while (enabled)
        {
            if (agent.isPlayerVisible)
                seenTimeDiff += Time.deltaTime * seenTimeMultiplier;

            if (seenTimeDiff >= secondsUntillMoveToTarget)
            {
                if (!isMovingToTarget)
                {
                    /*TODO: Show yellow arrow above head, play distract animation*/

                    isMovingToTarget = true;
                    transform.LookAt(agent.lastKnownPlayerPosition);
                    agent.navMeshAgent.SetDestination(agent.lastKnownPlayerPosition);
                }
            }
            
            if (seenTimeDiff >= secondsUntillChase)
                agent.fsm.ChangeState<EnemyStateChasePlayer>();

            yield return null;
        }
    }

    IEnumerator UpdateSeenTimeMultiplier()
    {
        yield return new WaitForSeconds(0.5f);

        while (enabled)
        {
            Vector3 toOther = agent.targetTransform.position - transform.position;
            toOther = toOther.ProjectOntoPlane(Vector3.up);

            bool canIncreaseMultiplier =
                Vector3.Angle(toOther, transform.forward) < agent.fov.maxAngle * 0.3f;

            if (canIncreaseMultiplier)
            {
                float distance = toOther.magnitude;
                float bonusMultiplier = 0.02f;
                seenTimeMultiplier += Mathf.Min(0.2f, (bonusMultiplier * distance / 100.0f));
            }
            else seenTimeMultiplier = 1.0f;

            yield return null;
        }
    }
}
