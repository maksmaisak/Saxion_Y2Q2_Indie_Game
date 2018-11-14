using System;
using UnityEngine;

public static class TimeHelper
{
    /// Same as Time.timeScale, but also adjusts Time.fixedDeltaTime when set.
    public static float timeScale
    {
        get
        {
            return Time.timeScale;
        }
        set
        {
            Time.timeScale = value;
            Time.fixedDeltaTime = defaultFixedDeltaTime * value;
        }
    }

    private static readonly float defaultFixedDeltaTime = Time.fixedDeltaTime;
}