using UnityEngine;

public class EnemyStateIdle : FSMState<Enemy>, IEventReceiver<Disturbance>
{
    [SerializeField] float disturbanceHearingRadius = 10f;
    [SerializeField] float rotationSpeed = 100f;
    [SerializeField] float rotationSpeedChange = 100f;
    [SerializeField] float rotationSpeedCap = 100f;
        
    public void On(Disturbance shot)
    {
        if ((shot.position - transform.position).sqrMagnitude > disturbanceHearingRadius * disturbanceHearingRadius) 
            return;
        
        agent.lastHeardDisturbancePositions = shot.position;
        
        if (enabled)
        {
            agent.fsm.ChangeState<EnemyStateInvestigateDisturbance>();
        }
    }

    private void OnEnable()
    {
        if (Random.value < 0.5f)
        {
            rotationSpeed = -rotationSpeed;
        }
    }

    void Update()
    {
        rotationSpeed += Random.Range(-rotationSpeedChange, rotationSpeedChange) * Time.deltaTime;
        rotationSpeed = Mathf.Clamp(rotationSpeed, -rotationSpeedCap, rotationSpeedCap);
        
        transform.rotation *= Quaternion.AngleAxis(rotationSpeed * Time.deltaTime, Vector3.up);
    }
}