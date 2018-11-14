using UnityEngine;
using System.Collections;

public class PersistentSingleton<T> : MyBehaviour where T : PersistentSingleton<T>
{
    private static T instanceCached;

    public static T instance
    {
        get
        {
            if (isApplicationQuitting) return null;

            if (instanceCached == null)
            {
                instanceCached = FindInstance() ?? CreateInstance();
            }

            return instanceCached;
        }
    }

    private static bool isApplicationQuitting;

    private static T FindInstance()
    {
        return FindObjectOfType<T>();
    }

    private static T CreateInstance()
    {
        var gameObject = new GameObject("(Persistent Singleton) " + typeof(T));
        DontDestroyOnLoad(gameObject);
        return gameObject.AddComponent<T>();
    }

    protected virtual void OnDestroy()
    {
        isApplicationQuitting = true;
    }
}