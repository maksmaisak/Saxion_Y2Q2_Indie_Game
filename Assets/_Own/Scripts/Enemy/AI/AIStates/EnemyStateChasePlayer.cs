using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;
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

    private bool isAttackingInMelee;
    private bool isShooting;

    void OnEnable()
    {
        agent.navMeshAgent.speed   = agent.chaseSpeed;
        agent.minimumAwarenessLevelThreshold = minimumChaseTimeThreshold;

        agent.SetNoCallAssistance(true);

        StartCoroutine(MoveCoroutine());
        StartCoroutine(AttackCoroutine());
        StartCoroutine(ShootCoroutine());
    }

    void OnDisable()
    {
        StopAllCoroutines();
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
            if (!isShooting && IsCloserToTargetThan(maxMeleeStartDistance))
            {
                isAttackingInMelee = true;
                yield return new WaitForSeconds(meleeTime);

                if (IsCloserToTargetThan(maxMeleeDamageDistance))
                    DealDamage();
                
                isAttackingInMelee = false;
            }

            yield return null;
        }
    }

    private IEnumerator ShootCoroutine()
    {
        while (true)
        {
            if (!isAttackingInMelee && !IsCloserToTargetThan(minShootingDistance) && agent.shootingController.IsClearPath(agent.targetTransform.gameObject))
            {
                isShooting = true;
                if (agent.shootingController.ShootAt(agent.targetTransform.gameObject))
                {
                    yield return new WaitForSeconds(shootingCooldown);
                }
                isShooting = false;
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

        health.DealDamage(meleeDamage);
    }
}