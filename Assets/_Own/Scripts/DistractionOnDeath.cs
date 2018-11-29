using System;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Health))]
public class DistractionOnDeath : MonoBehaviour
{
    [SerializeField] float distractionPriority = 6f;
    [SerializeField] bool overrideEnemyHearingRadius = true;
    [SerializeField] float enemyHearingRadius = 15f;

    void Start()
    {
        var health = GetComponent<Health>();
        health.OnDeath += sender =>
        {
            new Distraction(
                transform.position, 
                distractionPriority, 
                overrideEnemyHearingRadius ? (float?)enemyHearingRadius : null
            ).PostEvent();
        };
    }
}
