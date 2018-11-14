using System;

/// WARNING: THIS IS BEING PHASED OUT
public static class SceneNames
{
    // This stuff has to be hardcoded since you can't just assign scenes in the inspector.
    // Still better to keep it centralized anyway. A solution using ScriptableObjects might be
    // possible, but there's no time.
    
    public const string preload = "__preload";
    public const string mainMenuName = "main_menu";
    public const string mainLevelName = "main";
    public const string mainResolutionScreenName = "main_resolution_screen";
}
