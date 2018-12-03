using UnityEngine;
using UnityEngine.Events;

/// Allows setting up handlers to broadcast events in the editor.
public class CustomEventReceiver : MyBehaviour,
    IEventReceiver<OnLevelCompleted>
{
    [SerializeField] UnityEvent OnLevelCompletedHandlers;
    public void On(OnLevelCompleted message) => OnLevelCompletedHandlers.Invoke();
}