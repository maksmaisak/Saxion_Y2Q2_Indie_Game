using UnityEngine;

public class EnemyStateIdle : FSMState<EnemyAI>
{
    [SerializeField] float rotationSpeed = 100f;
    [SerializeField] float rotationSpeedChange = 100f;
    [SerializeField] float rotationSpeedCap = 100f;
    [SerializeField] float minDelayToPatrole = 2.0f;
    [SerializeField] float maxDelayToPatrole = 10.0f;
    
    private float delayToPatrole = 2.0f;
    
    private void OnEnable()
    {
        agent.canInvestigateDisturbance = true;
        agent.minimumTimeTreshold = 0.0f;

        if (Random.value < 0.5f)
            rotationSpeed = -rotationSpeed;

        delayToPatrole = Random.Range(minDelayToPatrole, maxDelayToPatrole);

        this.Delay(delayToPatrole, agent.fsm.ChangeState<EnemyStatePatrol>);
    }

    void Update()
    {
        rotationSpeed       += Random.Range(-rotationSpeedChange, rotationSpeedChange) * Time.deltaTime;
        rotationSpeed       = Mathf.Clamp(rotationSpeed, -rotationSpeedCap, rotationSpeedCap);
        transform.rotation  *= Quaternion.AngleAxis(rotationSpeed * Time.deltaTime, Vector3.up);
    }

    private void OnDisable()
    {
        agent.canInvestigateDisturbance = false;
    }
}