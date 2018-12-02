using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

[CustomEditor(typeof(EnemyAI), editorForChildClasses: true)]
[CanEditMultipleObjects]
public class EnemyAIEditor : Editor
{
    private SerializedProperty maxViewAngle;
    private SerializedProperty maxViewDistance;
    private SerializedProperty fullViewRadius;
    private SerializedProperty visionOrigin;
    
    private SerializedProperty footstepsHearingRadius;
    private SerializedProperty footstepsHearingRadiusWhileCovered;
    
    private readonly Color visionSurfaces = new Color(1f, 0f, 0f, 0.02f); 
    private readonly Color visionHandles  = new Color(1f, 0f, 0f, 1f   ); 
    
    private readonly Color hearingSurfaces = new Color(0f, 0f, 1f, 0.02f); 
    private readonly Color hearingHandles  = new Color(0f, 0f, 1f, 1f   ); 

    void OnEnable()
    {
        maxViewAngle    = serializedObject.FindProperty(nameof(maxViewAngle   ));
        maxViewDistance = serializedObject.FindProperty(nameof(maxViewDistance));
        fullViewRadius  = serializedObject.FindProperty(nameof(fullViewRadius ));
        visionOrigin    = serializedObject.FindProperty(nameof(visionOrigin   ));
        
        footstepsHearingRadius             = serializedObject.FindProperty(nameof(footstepsHearingRadius));
        footstepsHearingRadiusWhileCovered = serializedObject.FindProperty(nameof(footstepsHearingRadiusWhileCovered));
    }

    void OnSceneGUI()
    {
        if (visionOrigin.hasMultipleDifferentValues) return;
                    
        Transform transform = (Transform)visionOrigin.objectReferenceValue;
        Vector3 position = transform.position;
        Vector3 up = transform.up;
        Vector3 forward = transform.forward;
        Quaternion rotation = transform.rotation;
        
        Handles.zTest = CompareFunction.LessEqual;
        
        EditorGUI.BeginChangeCheck();

        Handles.color = visionSurfaces;
        DrawViewArc(
            position, 
            up, 
            forward,
            maxViewAngle.floatValue,
            maxViewDistance.floatValue
        );
        Handles.color = visionHandles;
        maxViewDistance.floatValue = Handles.RadiusHandle(rotation, position, maxViewDistance.floatValue);
            
        Handles.color = visionSurfaces;
        DrawViewArc(
            position, 
            up, 
            -forward,
            360f - maxViewAngle.floatValue,
            fullViewRadius.floatValue
        );
        Handles.color = visionHandles;
        fullViewRadius.floatValue = Handles.RadiusHandle(rotation, position, fullViewRadius.floatValue);

        Handles.color = hearingSurfaces;
        Handles.DrawSolidDisc(position, up, footstepsHearingRadius.floatValue);
        
        Handles.color = hearingHandles;
        footstepsHearingRadius.floatValue = Handles.RadiusHandle(rotation, position, footstepsHearingRadius.floatValue);

        Handles.DrawWireDisc(position, up, footstepsHearingRadiusWhileCovered.floatValue);
        footstepsHearingRadiusWhileCovered.floatValue = Handles.RadiusHandle(rotation, position, footstepsHearingRadiusWhileCovered.floatValue);

        serializedObject.ApplyModifiedProperties();
    }
    
    void DrawViewArc(Vector3 position, Vector3 up, Vector3 forward, float angle, float radius)
    {
        Handles.DrawSolidArc(
            position,
            up,
            Quaternion.AngleAxis(-angle / 2, up) * forward,
            angle,
            radius
        );
    }
}