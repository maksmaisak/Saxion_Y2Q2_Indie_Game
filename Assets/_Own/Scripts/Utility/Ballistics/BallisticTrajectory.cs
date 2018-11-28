using System;
using UnityEngine;

public struct BallisticTrajectory
{
    public readonly Vector3 startPosition;
    public readonly Vector3 startVelocity;
    public readonly float totalTime;

    public BallisticTrajectory(Vector3 startPosition, Vector3 startVelocity, float totalTime)
    {
        this.startPosition = startPosition;
        this.startVelocity = startVelocity;
        this.totalTime = totalTime;
    }

    public Vector3 GetPositionAt(float time)
    {
        return startPosition + time * startVelocity + time * time * Physics.gravity * 0.5f;
    }

    public bool CheckIsClear(LayerMask obstacleDetectionLayerMask, GameObject target, int numSegments = 10, float sphereCastRadius = 0.1f)
    {
        Vector3 previousPosition = GetPositionAt(0f);
        for (int i = 0; i < numSegments; ++i)
        {
            float time = totalTime * i / (numSegments + 1);
            Vector3 position = GetPositionAt(time);
            if (!CheckIsSegmentClear(previousPosition, position, sphereCastRadius, obstacleDetectionLayerMask, target)) return false;
        }

        return true;
    }

    private bool CheckIsSegmentClear(Vector3 pointA, Vector3 pointB, float sphereCastRadius, LayerMask obstacleDetectionLayerMask, GameObject target)
    {
        Vector3 delta = pointB - pointA;
        float distance = delta.magnitude;
        Vector3 direction = delta / distance;
        
        RaycastHit hit;
        bool didHit = Physics.SphereCast(
            origin: pointA,
            radius: sphereCastRadius,
            direction: direction,
            hitInfo: out hit,
            maxDistance: distance,
            layerMask: obstacleDetectionLayerMask,
            queryTriggerInteraction: QueryTriggerInteraction.Ignore
        );
        
        return !didHit || hit.collider.gameObject == target;
    }
}
