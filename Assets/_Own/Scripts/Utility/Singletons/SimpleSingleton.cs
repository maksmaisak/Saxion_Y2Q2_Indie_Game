using UnityEngine;
using System.Collections;

public class SimpleSingleton<T> where T : class, new()
{
    private static T _instance;

    public static T instance => _instance = _instance ?? new T();
    public static bool exists => _instance != null;
}
