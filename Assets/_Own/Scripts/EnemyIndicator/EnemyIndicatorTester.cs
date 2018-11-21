using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(EnemyIndicator))]
public class EnemyIndicatorTester : MonoBehaviour
{    
    IEnumerator Start()
    {
        EnemyIndicator indicator = GetComponent<EnemyIndicator>();
                
        while (enabled)
        {
            indicator.SetState(Random.Range(0f, 2f));
            yield return new WaitForSeconds(1f);
        }
    }
}