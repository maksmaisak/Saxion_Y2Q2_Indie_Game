using UnityEngine;

public static class Quit
{
    public static void ToDesktop() 
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }
}
