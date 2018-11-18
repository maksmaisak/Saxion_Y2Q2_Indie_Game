using UnityEngine;
using UnityEngine.AI;

public class EnemyStateWander : FSMState<Enemy>
{
    [SerializeField] float duration = 10f;
    [SerializeField] float circleDistance = 3f;
    [SerializeField] float circleRadius = 2f;
    [SerializeField] float angleChange = 10f;
    [SerializeField] float minDistanceToObstacle = 3f;

    float currentAngle = 0f;

    public Vector3 GetRelativeTargetPosition()
    {
        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();
        Vector3 circleDisplacement = forward * circleDistance;

        currentAngle += Random.Range(-angleChange / 2f, angleChange / 2f) * Time.deltaTime;
        Vector3 circleOutwardVector = Quaternion.AngleAxis(currentAngle, Vector3.up) * forward * circleRadius;

        return circleDisplacement + circleOutwardVector;
    }
    
    void OnEnable()
    {
        this.Delay(duration, () => agent.fsm.ChangeState<EnemyStateIdle>());
    }

    void Update()
    {
        Vector3 targetPosition = transform.position + GetRelativeTargetPosition();
        NavMeshHit hit;
        if (agent.navMeshAgent.Raycast(targetPosition, out hit))
        {
            targetPosition = hit.position;
        }

        if ((targetPosition - transform.position).sqrMagnitude < minDistanceToObstacle * minDistanceToObstacle)
        {
            currentAngle += 180f + Random.Range(-90f, 90f);
        }
               
        agent.navMeshAgent.SetDestination(targetPosition);
    }
}