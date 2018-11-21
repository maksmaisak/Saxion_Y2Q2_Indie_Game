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

    void Start()
    {
        Assert.IsNotNull(primaryVirtualCamera);
        Assert.IsNotNull(sniperZoomVirtualCamera);

        currentFov = primaryVirtualCamera.m_Lens.FieldOfView;
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
        }
        else if (previousFov <= sniperMaxFov && currentFov > sniperMaxFov)
        {
            isSniperZoom = false;
        }
        
        if (isSniperZoom)
        {
            currentFov = Mathf.Clamp(currentFov, sniperMinFov, sniperMaxFov);
            sniperZoomVirtualCamera.m_Lens.FieldOfView = Mathf.Clamp(currentFov, sniperMinFov, sniperMaxFov);
        }
        else
        {
            currentFov = Mathf.Clamp(currentFov, primaryMinFov, primaryMaxFov);
            primaryVirtualCamera.m_Lens.FieldOfView = Mathf.Clamp(currentFov, primaryMinFov, primaryMaxFov);
        }
        
        primaryVirtualCamera.Priority = isSniperZoom ? 0 : 10;
        sniperZoomVirtualCamera.Priority = isSniperZoom ? 10 : 0;
        
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = !isSniperZoom;
        }
    }
}