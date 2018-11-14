using System;
using System.Collections;
using UnityEngine;

public static class MonoBehaviourExtensions
{
    public static Coroutine Delay(this MonoBehaviour behaviour, float delay, Action action)
    {
        return behaviour.StartCoroutine(DelayCoroutine(delay, action));
    }

    private static IEnumerator DelayCoroutine(float delay, Action action)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }
    
    public static Coroutine DoNextFrame(this MonoBehaviour behaviour, Action action)
    {
        return behaviour.StartCoroutine(DoNextFrameCoroutine(action));
    }
    
    private static IEnumerator DoNextFrameCoroutine(Action action)
    {
        yield return null;
        action?.Invoke();
    }
}