using UnityEngine;
using System.Collections;
using System;

public abstract class Singleton<T> : MyBehaviour where T : Singleton<T>
{
    private static T instanceCached;
    public static T instance
    {
        get
        {
            if (instanceCached == null)
            {
                instanceCached = FindInstance() ?? CreateInstance();
            }

            return instanceCached;
        }
    }

    public static bool exists
    {
        get
        {
            if (instanceCached == null)
            {
                instanceCached = FindInstance();
            }

            return instanceCached;
        }
    }

    private static T FindInstance()
    {
        return FindObjectOfType<T>();
    }

    private static T CreateInstance()
    {
        var gameObject = new GameObject(typeof(T).ToString());
        gameObject.transform.SetAsFirstSibling();
        return gameObject.AddComponent<T>();
    }
}
