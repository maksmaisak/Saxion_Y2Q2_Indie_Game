using UnityEngine;
using System.Collections;
using System;
using UnityEngine.SceneManagement;

public class SceneHelper : PersistentSingleton<SceneHelper>
{
    public event Action OnActiveSceneChange;

    private int currentSceneBuildIndex = -1;

    protected override void Awake()
    {
        base.Awake();
        
        currentSceneBuildIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    void OnActiveSceneChanged(Scene currentScene, Scene nextScene)
    {
        if (currentSceneBuildIndex == nextScene.buildIndex) return;
        
        currentSceneBuildIndex = nextScene.buildIndex;
        OnActiveSceneChange?.Invoke();
    }
}