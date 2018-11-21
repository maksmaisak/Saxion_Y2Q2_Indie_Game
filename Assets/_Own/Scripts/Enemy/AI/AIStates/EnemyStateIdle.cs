using UnityEngine;

public class EnemyStateIdle : FSMState<EnemyAI>
{
    [SerializeField] float rotationSpeed = 100f;
    [SerializeField] float rotationSpeedChange = 100f;
    [SerializeField] float rotationSpeedCap = 100f;

    private void OnEnable()
    {
        if (Random.value < 0.5f)
            rotationSpeed = -rotationSpeed;

        agent.SetAIState(AIState.Idle);
    }

    void Update()
    {
        rotationSpeed += Random.Range(-rotationSpeedChange, rotationSpeedChange) * Time.deltaTime;
        rotationSpeed = Mathf.Clamp(rotationSpeed, -rotationSpeedCap, rotationSpeedCap);

        transform.rotation *= Quaternion.AngleAxis(rotationSpeed * Time.deltaTime, Vector3.up);
    }
}