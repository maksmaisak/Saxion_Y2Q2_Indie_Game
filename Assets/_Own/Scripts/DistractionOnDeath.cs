using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Health))]
public class DistractionOnDeath : MonoBehaviour
{
    [SerializeField] float distractionPriority = 6f;
    [SerializeField] float enemyHearingRadius = 15f;
    
#if UNITY_EDITOR
    [CustomEditor(typeof(DistractionOnDeath))]
    public class DistractionOnDeathEditor : Editor
    {
        void OnSceneGUI()
        {
            var distractionOnDeath = (DistractionOnDeath)target;            
            EditorUtils.UpdateHearingRadiusWithHandles(
                distractionOnDeath, 
                distractionOnDeath.transform, 
                ref distractionOnDeath.enemyHearingRadius
            );
        }
    }
#endif

    void Start()
    {
        var health = GetComponent<Health>();
        health.OnDeath += sender =>
        {
            new Distraction(transform.position, distractionPriority, enemyHearingRadius).PostEvent();
        };
    }
}
