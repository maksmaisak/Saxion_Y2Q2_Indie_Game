using System;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Health))]
public class DistractionOnDeath : MonoBehaviour
{
    [SerializeField] float distractionLoudness = 0.7f;
    [SerializeField] float distractionPriority = 6.0f;

    void Start()
    {
        var health = GetComponent<Health>();
        health.OnDeath += sender =>
        {
            new Distraction(transform.position, distractionPriority, distractionLoudness).PostEvent();
        };
    }
}
