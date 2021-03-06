using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class EnemyStateChasePlayer : FSMState<EnemyAI>
{
    [SerializeField] float minimumChaseTimeThreshold = 2.0f;
    [SerializeField] float secondsToEvadeMode = 3.0f;
    [SerializeField] float faceTargetSpeed = 10.0f;
    [Header("Melee attack")]
    [SerializeField] float maxMeleeStartDistance = 2f;
    [SerializeField] float maxMeleeDamageDistance = 2f;
    [SerializeField] float meleeTime = 1f;
    [SerializeField] int meleeDamage = 100;
    [Header("Ranged attack")] 
    [SerializeField] float shootingCooldown = 1f;
    [SerializeField] float minShootingDistance = 10f;
    [SerializeField] [Range(0f, 1f)] float rangedAttackProbabilityPerSecond = 0.5f;
    [SerializeField] [Range(0f, 1f)] float firstRangedAttackProbability = 0.8f;
    [Space]
    [SerializeField] UnityEvent OnEnterChaseMode;
    [SerializeField] UnityEvent OnMeleeAttack;
    [SerializeField] UnityEvent OnThrow;

    private bool isAttacking;

    void OnEnable()
    {
        OnEnterChaseMode.Invoke();
        
        agent.navMeshAgent.speed                = agent.chaseSpeed;
        agent.minimumAwarenessLevelThreshold    = minimumChaseTimeThreshold;

        agent.SetNoCallAssistance(true);
        agent.SetInvestigateNewDisturbance(false);

        agent.awarenessLevelIndicator.ShowAlertDetectedPlayer();

        StartCoroutine(MoveCoroutine());
        StartCoroutine(MeleeAttackCoroutine());
        StartCoroutine(RangedAttackCoroutine());

        new OnEnemyCombat(agent, true).SetDeliveryType(MessageDeliveryType.Immediate).PostEvent();
    }

    void OnDisable()
    {
        StopAllCoroutines();

        agent.SetInvestigateNewDisturbance(true);
        agent.SetNoCallAssistance(false);
        
        new OnEnemyCombat(agent, false).SetDeliveryType(MessageDeliveryType.Immediate).PostEvent();
    }

    private IEnumerator MoveCoroutine()
    {
        while (true)
        {
            agent.navMeshAgent.SetDestination(agent.lastKnownPlayerPosition);

            if (!agent.navMeshAgent.pathPending && agent.navMeshAgent.remainingDistance < agent.navMeshAgent.stoppingDistance)
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

    private IEnumerator MeleeAttackCoroutine()
    {
        while (true)
        {
            if (!isAttacking && IsCloserToTargetThan(maxMeleeStartDistance))
            {
                isAttacking = true;
                OnMeleeAttack.Invoke();
                yield return new WaitForSeconds(meleeTime);

                if (IsCloserToTargetThan(maxMeleeDamageDistance))
                    DealDamage();
                
                isAttacking = false;
            }

            yield return null;
        }
    }

    private IEnumerator RangedAttackCoroutine()
    {
        bool isFirstShot = true;
        
        while (true)
        {
            float chance = firstRangedAttackProbability;

            if (isFirstShot)
            {
                // Adjust for multiple checks per second.
                chance = 1f - Mathf.Pow(1f - rangedAttackProbabilityPerSecond, Time.deltaTime);
            }

            if (!isAttacking && Random.value < chance && !IsCloserToTargetThan(minShootingDistance))
            {
                isAttacking = true;
                if (agent.shootingController.ShootAt(agent.targetTransform.gameObject))
                {
                    isFirstShot = false;
                    OnThrow.Invoke();
                    yield return new WaitForSeconds(shootingCooldown);
                }
                isAttacking = false;
            }

            yield return null;
        }
    }
    
    private bool IsCloserToTargetThan(float maxDistance)
    {
        if (agent.isPlayerVisible)
            return agent.navMeshAgent.remainingDistance < maxDistance;

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

        health.DealDamage(meleeDamage);
    }
}