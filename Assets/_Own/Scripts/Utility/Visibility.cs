using Cinemachine.Utility;
using UnityEngine;

public class Visibility {

    public static bool CanSeeObject(Transform ownTransform, Transform targetTransform, FovInfo fov, float radius = 0f)
    {       
        Vector3 ownPosition = ownTransform.position;
        Vector3 toOther = targetTransform.position - ownPosition;
        float toOtherSqrMagnitude = toOther.sqrMagnitude;
        
        bool isWithinCloseDistance = toOtherSqrMagnitude < radius * radius;

        if (!isWithinCloseDistance)
        {
            if (toOtherSqrMagnitude > fov.maxDistance * fov.maxDistance)
                return false;
            if (Vector3.Angle(toOther, ownTransform.forward) > fov.maxAngle * 0.5f)
                return false;
        }

        RaycastHit hit;
        if (!Physics.Raycast(ownPosition, toOther, out hit, fov.maxDistance, fov.layerMask))
        {
            Debug.DrawRay(ownPosition, toOther);
            return true;
        }
        
        Debug.DrawRay(ownPosition, toOther.normalized * hit.distance);
        return hit.distance * hit.distance > toOtherSqrMagnitude;
    }
}
