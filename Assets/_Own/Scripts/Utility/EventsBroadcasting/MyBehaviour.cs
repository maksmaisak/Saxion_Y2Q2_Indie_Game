using System;
using UnityEngine;

/// A behaviour that can receive broadcast events.
public abstract class MyBehaviour : MonoBehaviour
{
    protected virtual void Awake()
    {
        if (!EnsurePreloadScene.wasPreloadLoaded)
        {
            // Wait for the preload scene to load the EventsManager.
            this.DoNextFrame(Subscribe);
        }
        else
        {
            Subscribe();
        }
    }

    protected virtual void OnDestroy()
    {
        var manager = EventsManager.instance;
        if (manager) manager.Remove(this);
    }

    private void Subscribe()
    {
        EventsManager.instance.Add(this);
    }
}