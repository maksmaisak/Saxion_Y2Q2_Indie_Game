using System.Collections;
using System.Collections.Generic;
using Cinemachine.Utility;
using UnityEngine;

/// Sets the position of its gameobject to where the mouse is.
public class MouseTracker : MonoBehaviour
{
    private const float maxRaycastDistance = 1000f;
    
    void Update()
    {
        Camera camera = Camera.main;
        if (camera == null) return;
        
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * 20f);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, maxRaycastDistance, Physics.DefaultRaycastLayers))
        {
            transform.position = new Vector3(hit.point.x, 0f, hit.point.z);
        }
        else
        {
            transform.position = ray.GetPoint(camera.transform.position.y / Vector3.Dot(camera.transform.forward, ray.direction));
        }
    }
}