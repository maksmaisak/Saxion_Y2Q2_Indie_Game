using UnityEngine;

public class EnemyStateLookAround : FSMState<Enemy>
{
    [SerializeField] float duration = 4f;
    [SerializeField] float rotationPerSecond = 90f;
    
    void OnEnable()
    {
        this.Delay(duration, () => agent.fsm.ChangeState<EnemyStateWander>());
    }
    
    void Update()
    {        
        transform.rotation *= Quaternion.AngleAxis(rotationPerSecond * Time.deltaTime, Vector3.up);
    }
}