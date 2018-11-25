﻿using UnityEngine;
using System.Collections;
using System;
using UnityEngine.SceneManagement;

public class ActiveSceneChanged : BroadcastEvent<ActiveSceneChanged> {}

public class SceneHelper : SimpleSingleton<SceneHelper>
{
    public event Action OnActiveSceneChange;

    private int currentSceneBuildIndex = -1;

    public SceneHelper()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    void OnActiveSceneChanged(Scene currentScene, Scene nextScene)
    {
        if (currentSceneBuildIndex == -1)
        {
            currentSceneBuildIndex = nextScene.buildIndex;
            return;
        }

        if (currentSceneBuildIndex == nextScene.buildIndex) return;
        
        currentSceneBuildIndex = nextScene.buildIndex;
        OnActiveSceneChange?.Invoke();
        new ActiveSceneChanged()
            .SetDeliveryType(MessageDeliveryType.Immediate)
            .PostEvent();
    }
}