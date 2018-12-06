using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/// Sets the position of its gameobject to where the camera is aiming.
public class AimingTarget : MonoBehaviour
{
    [SerializeField] Transform cameraTransform;
    [SerializeField] float maxRaycastDistance = 4000f;
    [SerializeField] LayerMask raycastLayerMask = Physics.DefaultRaycastLayers;
    
    void Update()
    {
        transform.position = GetNextPosition();
    }

    private Vector3 GetNextPosition()
    {
        cameraTransform = cameraTransform ? cameraTransform : Camera.main?.transform;
        Assert.IsNotNull(cameraTransform);
        
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, maxRaycastDistance, raycastLayerMask, QueryTriggerInteraction.Ignore))
        {
            return hit.point;
        }
        
        return ray.GetPoint(maxRaycastDistance);
    }
}