using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

public class EnemyStateChasePlayer : FSMState<EnemyAI>
{
    [SerializeField] float minimumChaseTimeThreshold = 2.0f;
    [SerializeField] float secondsToEvadeMode = 3.0f;
    [SerializeField] float faceTargetSpeed = 10.0f;
    [Header("Attack")]
    [SerializeField] float maxAttackStartDistance = 2f;
    [SerializeField] float maxAttackDamageDistance = 2f;
    [SerializeField] float attackTime = 1f;
    [SerializeField] int attackDamage = 100;

    void OnEnable()
    {
        agent.navMeshAgent.speed   = agent.chaseSpeed;
        agent.minimumAwarenessLevelThreshold = minimumChaseTimeThreshold;

        agent.SetNoCallAssistance(true);
        agent.SetInvestigateNewDisturbance(false);

        StartCoroutine(MoveCoroutine());
        StartCoroutine(AttackCoroutine());
    }

    void OnDisable()
    {
        StopAllCoroutines();
        agent.SetInvestigateNewDisturbance(true);
        agent.SetNoCallAssistance(false);
    }

    private IEnumerator MoveCoroutine()
    {
        while (true)
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

                    transform.rotation = Quaternion.RotateTowards(
                        transform.rotation,
                        Quaternion.LookRotation(targetPosition - transform.position), 
                        Time.deltaTime * faceTargetSpeed
                    );
                }
            }

            yield return null;
        }
    }

    private IEnumerator AttackCoroutine()
    {
        while (true)
        {
            if (IsCloserToTargetThan(maxAttackStartDistance))
            {
                yield return new WaitForSeconds(attackTime);

                if (IsCloserToTargetThan(maxAttackDamageDistance))
                    DealDamage();
            }

            yield return null;
        }
    }

    private bool IsCloserToTargetThan(float maxDistance)
    {
        Vector3 toTarget = agent.targetTransform.position - transform.position;
        return toTarget.sqrMagnitude < maxDistance * maxDistance;
    }

    private void DealDamage()
    {
        var health = agent.targetTransform.GetComponentInChildren<Health>();
        if (!health) health = agent.targetTransform.GetComponentInParent<Health>();

        if (!health)
        {
            Debug.LogWarning("Couldn't find Health in hierarchy of tracked object " + agent.targetTransform.gameObject);
            return;
        }

        health.DealDamage(attackDamage);
    }
}