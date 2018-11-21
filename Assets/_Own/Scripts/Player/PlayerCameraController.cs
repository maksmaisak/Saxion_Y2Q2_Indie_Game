using UnityEngine;
using Cinemachine;
using UnityEngine.Assertions;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] CinemachineFreeLook primaryVirtualCamera;
    [SerializeField] CinemachineVirtualCamera sniperZoomVirtualCamera;
    [SerializeField] float zoomSpeed = 10f;
    [SerializeField] float primaryMaxFov = 40f;
    [SerializeField] float primaryMinFov = 40f;
    [SerializeField] float sniperMaxFov = 10f;
    [SerializeField] float sniperMinFov = 5f;

    [SerializeField] float currentFov;

    private Renderer[] renderers;
        
    void Start()
    {
        Assert.IsNotNull(primaryVirtualCamera);
        Assert.IsNotNull(sniperZoomVirtualCamera);

        currentFov = primaryVirtualCamera.m_Lens.FieldOfView;
        renderers = GetComponentsInChildren<Renderer>();
    }

    void Update()
    {
        float scrollAmount = Input.GetAxis("Mouse ScrollWheel");
        float zoomAmount = scrollAmount * zoomSpeed;

        float previousFov = currentFov;
        currentFov += zoomAmount;

        bool isSniperZoom = previousFov < primaryMinFov;
        if (previousFov >= primaryMinFov && currentFov < primaryMinFov)
        {
            isSniperZoom = true;
            PointSniperCameraAtMouse();
        }
        else if (previousFov <= sniperMaxFov && currentFov > sniperMaxFov)
        {
            isSniperZoom = false;
        }
        
        if (isSniperZoom)
        {
            currentFov = Mathf.Clamp(currentFov, sniperMinFov, sniperMaxFov);
            sniperZoomVirtualCamera.m_Lens.FieldOfView = currentFov;
        }
        else
        {
            currentFov = Mathf.Clamp(currentFov, primaryMinFov, primaryMaxFov);
            primaryVirtualCamera.m_Lens.FieldOfView = currentFov;
        }

        if (!isSniperZoom) PointSniperCameraAtMouse();
        
        primaryVirtualCamera.enabled = !isSniperZoom;
        sniperZoomVirtualCamera.enabled = isSniperZoom;
        //primaryVirtualCamera.Priority = isSniperZoom ? 0 : 10;
        //sniperZoomVirtualCamera.Priority = isSniperZoom ? 10 : 0;
        
        foreach (Renderer r in renderers)
        {
            r.enabled = !isSniperZoom;
        }
    }

    private void PointSniperCameraAtMouse()
    {
        if (!sniperZoomVirtualCamera.LookAt) return;
        
        var rotation = Quaternion.LookRotation(sniperZoomVirtualCamera.LookAt.position - sniperZoomVirtualCamera.transform.position);
        var pov = sniperZoomVirtualCamera.GetCinemachineComponent<CinemachinePOV>();

        var eulerAngles = rotation.eulerAngles;
        Debug.Log(eulerAngles);
        pov.m_VerticalAxis.Value = WrapEulerAngle(eulerAngles.x, pov.m_VerticalAxis.m_MinValue, pov.m_VerticalAxis.m_MaxValue);
        pov.m_HorizontalAxis.Value = WrapEulerAngle(eulerAngles.y, pov.m_HorizontalAxis.m_MinValue, pov.m_HorizontalAxis.m_MaxValue);
    }

    private static float WrapEulerAngle(float angle, float min, float max)
    {
        float delta = max - min;
        while (angle < min) angle += delta;
        while (angle > max) angle -= delta;
        return angle;
    }
}