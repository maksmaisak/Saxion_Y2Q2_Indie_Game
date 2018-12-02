using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

[CustomEditor(typeof(EnemyAI))]
[CanEditMultipleObjects]
public class EnemyAIEditor : Editor
{
    private SerializedProperty maxViewAngle;
    private SerializedProperty maxViewDistance;
    private SerializedProperty fullViewRadius;
    private SerializedProperty visionOrigin;

    void OnEnable()
    {
        maxViewAngle    = serializedObject.FindProperty(nameof(maxViewAngle   ));
        maxViewDistance = serializedObject.FindProperty(nameof(maxViewDistance));
        fullViewRadius  = serializedObject.FindProperty(nameof(fullViewRadius ));
        visionOrigin    = serializedObject.FindProperty(nameof(visionOrigin   ));
    }

    void OnSceneGUI()
    {
        var ai = (EnemyAI)target;

        if (!visionOrigin.hasMultipleDifferentValues)
        {
            Transform transform = (Transform)visionOrigin.objectReferenceValue;
            Vector3 position = transform.position;
            Vector3 up = transform.up;
            Vector3 forward = transform.forward;
            
            DrawViewArc(
                position, 
                up, 
                forward,
                maxViewAngle.floatValue,
                maxViewDistance.floatValue
            );
            
            DrawViewArc(
                position, 
                up, 
                -forward,
                360f - maxViewAngle.floatValue,
                fullViewRadius.floatValue
            );
            
            // TODO draw the covered footstep hearing range with a wire arc.
        }
    }

    void DrawViewArc(Vector3 position, Vector3 up, Vector3 forward, float angle, float radius)
    {
        Color oldColor = Handles.color;
        Handles.color = new Color(1f, 1f, 1f, 0.01f);
        Handles.zTest = CompareFunction.LessEqual;

        Handles.DrawSolidArc(
            position,
            up,
            Quaternion.AngleAxis(-angle / 2, up) * forward,
            angle,
            radius
        );

        Handles.color = oldColor;
    }

    void DrawHearing(Vector3 position, float radius)
    {
        //Handles.CircleHandleCap();
    }
}