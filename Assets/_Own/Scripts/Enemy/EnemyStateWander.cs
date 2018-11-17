using UnityEngine;
using UnityEngine.AI;

public class EnemyStateWander : FSMState<Enemy>
{
    void OnEnable()
    {
        // TEMP
        this.DoNextFrame(() => agent.fsm.ChangeState<EnemyStateIdle>());
    }
}