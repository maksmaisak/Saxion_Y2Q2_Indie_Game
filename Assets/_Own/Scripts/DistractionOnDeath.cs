using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Health))]
public class DistractionOnDeath : MonoBehaviour
{
    [SerializeField] float distractionPriority = 6f;
    [SerializeField] bool overrideEnemyHearingRadius = true;
    [SerializeField] float enemyHearingRadius = 15f;
    
    #if UNITY_EDITOR
    [CustomEditor(typeof(DistractionOnDeath))]
    public class DistractionOnDeathEditor : Editor
    {
        private readonly Color colorSurfaces = new Color(0f, 0f, 1f, 0.02f);
        private readonly Color colorHandles  = new Color(0f, 0f, 1f, 1f   );
        
        void OnSceneGUI()
        {
            var distractionOnDeath = (DistractionOnDeath)target;
            if (!distractionOnDeath.overrideEnemyHearingRadius) return;
            float hearingRadius = distractionOnDeath.enemyHearingRadius;

            Transform transform = distractionOnDeath.transform;
            Vector3 position = transform.position;
        
            Handles.color = colorSurfaces;
            Handles.DrawSolidDisc(position, transform.up, hearingRadius);
            
            EditorGUI.BeginChangeCheck();
            
            Handles.color = colorHandles;
            hearingRadius = Handles.RadiusHandle(
                transform.rotation, 
                position, 
                hearingRadius
            );
            
            if (!EditorGUI.EndChangeCheck()) return;
            
            Undo.RecordObject(distractionOnDeath, "Change distraction hearing radius.");
            distractionOnDeath.enemyHearingRadius = hearingRadius;
        }
    }
    #endif

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
