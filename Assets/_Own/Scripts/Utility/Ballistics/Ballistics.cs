using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Assertions;

public static class Ballistics {

    public static Vector3? GetStartVelocity(
        Vector3 start,
        Vector3 target,
        float muzzleSpeed,
        bool isHighTrajectory = false
    ) {
        
        Vector3 delta = target - start;
        Vector3 flatDelta = new Vector3(delta.x, 0f, delta.z);
        Vector3 flatDirection = flatDelta.normalized;

        float x = flatDelta.magnitude;
        float y = delta.y;
        float v = muzzleSpeed;
        float vSqr = v * v;
        float g = Mathf.Abs(Physics.gravity.y);

        float angle = isHighTrajectory ?
            Mathf.Atan((vSqr + Mathf.Sqrt(vSqr * vSqr - g * (g * x * x + 2f * y * vSqr))) / (x * g)) : 
            Mathf.Atan((vSqr - Mathf.Sqrt(vSqr * vSqr - g * (g * x * x + 2f * y * vSqr))) / (x * g));

        if (float.IsNaN(angle)) return null;

        Vector3 startVelocity = flatDirection * muzzleSpeed * Mathf.Cos(angle);
        startVelocity.y = muzzleSpeed * Mathf.Sin(angle);

        return startVelocity;
    }

    public struct BallisticTrajectoryPair
    {
        public BallisticTrajectory low;
        public BallisticTrajectory high;

        public BallisticTrajectoryPair(BallisticTrajectory low, BallisticTrajectory high)
        {
            this.low = low;
            this.high = high;
        }
    }

    public static BallisticTrajectoryPair? GetLowAndHighTrajectories(
        Vector3 start,
        Vector3 target,
        float muzzleSpeed
    )
    {
        Vector3 delta = target - start;
        Vector3 flatDelta = new Vector3(delta.x, 0f, delta.z);
        Vector3 flatDirection = flatDelta.normalized;

        float x = flatDelta.magnitude;
        float y = delta.y;
        float v = muzzleSpeed;
        float vSqr = v * v;
        float g = Mathf.Abs(Physics.gravity.y);

        float angleLow = Mathf.Atan((vSqr - Mathf.Sqrt(vSqr * vSqr - g * (g * x * x + 2f * y * vSqr))) / (x * g));
        if (float.IsNaN(angleLow)) return null;
        float angleHigh = Mathf.Atan((vSqr + Mathf.Sqrt(vSqr * vSqr - g * (g * x * x + 2f * y * vSqr))) / (x * g));

        Vector3 startVelocityLow = GetVelocity(flatDirection, angleLow, muzzleSpeed);
        float timeLow = (startVelocityLow.y + Mathf.Sqrt(startVelocityLow.y * startVelocityLow.y - 2 * g * y)) * 0.5f;

        Vector3 startVelocityHigh = GetVelocity(flatDirection, angleHigh, muzzleSpeed);
        float timeHigh = (startVelocityHigh.y + Mathf.Sqrt(startVelocityHigh.y * startVelocityHigh.y - 2 * g * y)) * 0.5f;
        
        return new BallisticTrajectoryPair(
            new BallisticTrajectory(start, startVelocityLow , timeLow),
            new BallisticTrajectory(start, startVelocityHigh, timeHigh)
        );
    }
    
    private static Vector3 GetVelocity(Vector3 flatDirection, float raiseAngle, float muzzleSpeed)
    {
        Assert.IsTrue(Mathf.Approximately(flatDirection.y, 0f));
        
        Vector3 startVelocity = flatDirection * muzzleSpeed * Mathf.Cos(raiseAngle);
        startVelocity.y = muzzleSpeed * Mathf.Sin(raiseAngle);
        return startVelocity;
    }
}
