using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(EnemyIndicator))]
public class EnemyIndicatorRandom : MonoBehaviour
{    
    IEnumerator Start()
    {
        EnemyIndicator indicator = GetComponent<EnemyIndicator>();

        Action[] actions =
        {
            indicator.SetStateIdle,
            indicator.SetStateSuspicious,
            indicator.SetStateAggressive
        };
        
        while (enabled)
        {
            yield return new WaitForSeconds(1f);
            actions[Random.Range(0, 3)]();
        }
    }
}