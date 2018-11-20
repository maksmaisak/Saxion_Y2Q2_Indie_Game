using Cinemachine.Utility;
using UnityEngine;

public class Visibility {

    public static bool CanSeeObject(Transform transform, Transform other, FovInfo fov)
    {       
        Vector3 ownPosition = transform.position;
        Vector3 toOther = other.position - ownPosition;
        toOther = toOther.ProjectOntoPlane(Vector3.up);

        if (toOther.sqrMagnitude > fov.maxDistance * fov.maxDistance) return false;
        if (Vector3.Angle(toOther, transform.forward) > fov.maxAngle * 0.5f) return false;

        RaycastHit hit;
        if (!Physics.Raycast(ownPosition, toOther, out hit, fov.maxDistance, fov.blockingLayerMask)) 
            return true;

        return hit.distance * hit.distance > toOther.sqrMagnitude;
    }
}
