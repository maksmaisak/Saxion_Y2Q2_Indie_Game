using System.Linq;
using UnityEngine;
using Cinemachine;
using UnityEngine.Assertions;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] CinemachineFreeLook primaryVirtualCamera;
    [SerializeField] CinemachineVirtualCamera sniperZoomVirtualCamera;
    [SerializeField] GameObject activeInThirdPersonOnly;
    [SerializeField] GameObject activeInSniperZoomOnly;
    [SerializeField] float zoomSpeed = 10f;
    [SerializeField] float primaryMaxFov = 40f;
    [SerializeField] float primaryMinFov = 40f;
    [SerializeField] float sniperMaxFov = 10f;
    [SerializeField] float sniperMinFov = 5f;

    [SerializeField] float currentFov;

    private bool isSniping = false;
    private Renderer[] renderers;
        
    void Start()
    {
        Assert.IsNotNull(primaryVirtualCamera);
        Assert.IsNotNull(sniperZoomVirtualCamera);

        currentFov = primaryVirtualCamera.m_Lens.FieldOfView;
        isSniping = currentFov < primaryMinFov;
        
        renderers = GetComponentsInChildren<Renderer>();
    }

    void Update()
    {
        float scrollAmount = Input.GetAxis("Mouse ScrollWheel");
        float zoomAmount = scrollAmount * zoomSpeed;

        currentFov += zoomAmount;

        if (!isSniping && currentFov < primaryMinFov) // From primary to sniper
        {
            isSniping = true;
            PointSniperCameraAtMouse();
        }
        else if (isSniping && currentFov > sniperMaxFov) // From sniper to primary
        {
            isSniping = false;
            PointPrimaryCameraAtMouse();
        }
        
        if (isSniping)
        {
            currentFov = Mathf.Clamp(currentFov, sniperMinFov, sniperMaxFov);
            sniperZoomVirtualCamera.m_Lens.FieldOfView = currentFov;
        }
        else
        {
            currentFov = Mathf.Clamp(currentFov, primaryMinFov, primaryMaxFov);
            primaryVirtualCamera.m_Lens.FieldOfView = currentFov;
        }
        
        primaryVirtualCamera.enabled = !isSniping;
        sniperZoomVirtualCamera.enabled = isSniping;
        
        if (!isSniping) PointSniperCameraAtMouse();
        else PointPrimaryCameraAtMouse();
        
        foreach (Renderer r in renderers) r.enabled = !isSniping;

        if (activeInThirdPersonOnly) activeInThirdPersonOnly.SetActive(!isSniping);
        if (activeInSniperZoomOnly) activeInSniperZoomOnly.SetActive(isSniping);
    }

    private void PointSniperCameraAtMouse()
    {
        Assert.IsNotNull(sniperZoomVirtualCamera.LookAt);
        
        var rotation = Quaternion.LookRotation(sniperZoomVirtualCamera.LookAt.position - sniperZoomVirtualCamera.transform.position);
        Vector3 eulerAngles = rotation.eulerAngles;
        var pov = sniperZoomVirtualCamera.GetCinemachineComponent<CinemachinePOV>();

        pov.m_VerticalAxis.Value = WrapEulerAngle(eulerAngles.x, pov.m_VerticalAxis.m_MinValue, pov.m_VerticalAxis.m_MaxValue);
        pov.m_HorizontalAxis.Value = WrapEulerAngle(eulerAngles.y, pov.m_HorizontalAxis.m_MinValue, pov.m_HorizontalAxis.m_MaxValue);
    }

    private void PointPrimaryCameraAtMouse()
    {
        Assert.IsNotNull(sniperZoomVirtualCamera.LookAt);
        
        var rotation = Quaternion.LookRotation(sniperZoomVirtualCamera.LookAt.position - primaryVirtualCamera.transform.position);
        Vector3 eulerAngles = rotation.eulerAngles;
        CinemachineFreeLook cam = primaryVirtualCamera;

        // OPTIMIZATION Cache this.
        float maxAngle = Mathf.Atan(cam.m_Orbits.Max(o => o.m_Height / o.m_Radius)) * Mathf.Rad2Deg;
        float minAngle = Mathf.Atan(cam.m_Orbits.Min(o => o.m_Height / o.m_Radius)) * Mathf.Rad2Deg;

        float yAxisAngle = WrapEulerAngle(eulerAngles.x, -90f, 90f);
        float yAxisValue = Remap(minAngle, maxAngle, cam.m_YAxis.m_MinValue, cam.m_YAxis.m_MaxValue, yAxisAngle);
        cam.m_YAxis.Value = Mathf.Clamp01(yAxisValue);
        
        cam.m_XAxis.Value = WrapEulerAngle(eulerAngles.y, cam.m_XAxis.m_MinValue, cam.m_XAxis.m_MaxValue);
    }

    private static float WrapEulerAngle(float angle, float min, float max)
    {
        float delta = max - min;
        while (angle < min) angle += delta;
        while (angle > max) angle -= delta;
        return angle;
    }
    
    /// Value in range (aMin, aMax) -> value in range (bMin, bMax)
    private static float Remap(float aMin, float aMax, float bMin, float bMax, float value) {
        return Mathf.Lerp(bMin, bMax, Mathf.InverseLerp(aMin, aMax, value));
    }
}