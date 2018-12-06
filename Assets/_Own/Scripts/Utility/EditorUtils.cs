using UnityEngine;
using UnityEditor;

// The reason we don't just put in an Editor folder is because then
// code outside of such folders wouldn't be able to access this class.
// Custom editors as nested classes and the like.
#if UNITY_EDITOR

public static class EditorUtils
{
    private static readonly Color colorSurfaces = new Color(0f, 0f, 1f, 0.02f);
    private static readonly Color colorHandles  = new Color(0f, 0f, 1f, 1f   );
    
    public static void UpdateHearingRadiusWithHandles(Object obj, Transform transform, ref float hearingRadius)
    {
        float hearingRadiusCopy = hearingRadius;
        Vector3 position = transform.position;
        
        Handles.color = colorSurfaces;
        Handles.DrawSolidDisc(position, Vector3.up, hearingRadiusCopy);
            
        EditorGUI.BeginChangeCheck();
            
        Handles.color = colorHandles;
        hearingRadiusCopy = Handles.RadiusHandle(
            transform.rotation, 
            position, 
            hearingRadiusCopy
        );
            
        if (!EditorGUI.EndChangeCheck()) return;
            
        Undo.RecordObject(obj, "Change distraction hearing radius.");
        hearingRadius = hearingRadiusCopy;
    }
}

#endif