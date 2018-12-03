using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnEnemyCombat : BroadcastEvent<OnEnemyCombat>
{
    public readonly EnemyAI agent;
    public readonly bool enterCombat;

    public OnEnemyCombat(EnemyAI agent, bool enterCombat) {
        this.agent          = agent;
        this.enterCombat    = enterCombat;
    }
}
