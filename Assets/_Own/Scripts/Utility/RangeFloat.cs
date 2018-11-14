using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public struct RangeFloat
{
    public float min;
    public float max;

    public float range  => max - min;
    public float middle => (min + max) * 0.5f;

    public RangeFloat(float min, float max)
    {
        if (min > max)
        {
            this.min = max;
            this.max = min;
            return;
        }
        
        this.min = min;
        this.max = max;
    }
    
    public bool Contains(float value)
    {
        return min <= value && max >= value;
    }

    public bool Contains(float value, float margin)
    {
        return
            min - margin <= value &&
            max + margin >= value;
    }
    
    public float Lerp(float t)
    {
        return Mathf.LerpUnclamped(min, max, t);
    }

    public float InverseLerp(float value)
    {
        return Mathf.InverseLerp(min, max, value);
    }
}

public static class RangeFloatExtensions
{
    public static RangeFloat Inflated(this RangeFloat range, float margin)
    {
        return new RangeFloat(range.min - margin, range.max + margin);
    }
    
    public static RangeFloat Inflated(this RangeFloat range, float marginMin, float marginMax)
    {
        return new RangeFloat(range.min - marginMin, range.max + marginMax);
    }
    
    public static bool Intersects(this RangeFloat a, RangeFloat b)
    {
        if (a.min <= b.min && b.max <= a.max) return true;
        if (b.min <= a.min && a.max <= b.max) return true;

        return a.Contains(b.min) || a.Contains(b.max);
    }

    public static bool Intersects(this RangeFloat a, RangeFloat b, float margin)
    {
        return a.Inflated(margin).Intersects(b);
    }
}