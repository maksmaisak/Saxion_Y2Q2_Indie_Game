using System.Linq;
using UnityEngine;
using Cinemachine;
using Cinemachine.Utility;
using UnityEngine.Animations;
using UnityEngine.Assertions;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] CinemachineFreeLook primaryVirtualCamera;
    [SerializeField] CinemachineVirtualCamera sniperZoomVirtualCamera;
    [SerializeField] GameObject activeInThirdPersonOnly;
    [SerializeField] GameObject activeInSniperZoomOnly;
    [SerializeField] Transform mouse;
    [SerializeField] float zoomSpeed = 10f;
    [SerializeField] float primaryMaxFov = 40f;
    [SerializeField] float primaryMinFov = 40f;
    [SerializeField] float sniperMaxFov = 10f;
    [SerializeField] float sniperMinFov = 5f;
    [SerializeField] bool invertScroll = false;

    [SerializeField] float currentFov;

    private bool isSniping = false;
    private CinemachineVirtualCamera hardLookAtMouseSniperCamera;
    private CinemachineVirtualCamera hardLookAtMousePrimaryCamera;
    private Renderer[] renderers;
        
    void Start()
    {
        Assert.IsNotNull(primaryVirtualCamera);
        Assert.IsNotNull(sniperZoomVirtualCamera);
        
        if (!mouse) mouse = FindObjectOfType<AimingTarget>().transform;
        Assert.IsNotNull(mouse);

        var go = Instantiate(sniperZoomVirtualCamera.gameObject, sniperZoomVirtualCamera.transform.parent);
        go.name = "CM HardLookAtMouseSniperCamera";
        hardLookAtMouseSniperCamera = go.GetComponent<CinemachineVirtualCamera>();
        hardLookAtMouseSniperCamera.Priority = -1;
        hardLookAtMouseSniperCamera.m_StandbyUpdate = CinemachineVirtualCameraBase.StandbyUpdateMode.Always;
        hardLookAtMouseSniperCamera.AddCinemachineComponent<CinemachineHardLookAt>();
        hardLookAtMouseSniperCamera.DestroyCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
       
        go = new GameObject("HardLookAtMouse");
        go.transform.SetParent(primaryVirtualCamera.transform, worldPositionStays: false);
        hardLookAtMousePrimaryCamera = go.AddComponent<CinemachineVirtualCamera>();
        hardLookAtMousePrimaryCamera.Priority = -1;
        hardLookAtMousePrimaryCamera.m_StandbyUpdate = CinemachineVirtualCameraBase.StandbyUpdateMode.Always;
        hardLookAtMousePrimaryCamera.LookAt = mouse;
        hardLookAtMousePrimaryCamera.AddCinemachineComponent<CinemachineHardLookAt>();

        currentFov = primaryVirtualCamera.m_Lens.FieldOfView;
        isSniping = currentFov < primaryMinFov;

        sniperZoomVirtualCamera.LookAt = mouse;
        
        renderers = GetComponentsInChildren<Renderer>();
    }

    void Update()
    {
        float scrollAmount = Input.GetAxis("Mouse ScrollWheel");
        float zoomAmount = -scrollAmount * zoomSpeed;
        if (invertScroll) zoomAmount = -zoomAmount;

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
        
        if (isSniping) currentFov = Mathf.Clamp(currentFov, sniperMinFov, sniperMaxFov);
        else currentFov = Mathf.Clamp(currentFov, primaryMinFov, primaryMaxFov);
        
        primaryVirtualCamera.m_Lens.FieldOfView = sniperZoomVirtualCamera.m_Lens.FieldOfView = currentFov;

        if (isSniping)
        {
            sniperZoomVirtualCamera.Priority = 10;
            primaryVirtualCamera.Priority = 1;
        }
        else
        {
            sniperZoomVirtualCamera.Priority = 1;
            primaryVirtualCamera.Priority = 10;
        }
        
        if (isSniping) PointPrimaryCameraAtMouse();
        else PointSniperCameraAtMouse();
        
        foreach (Renderer r in renderers) r.enabled = !isSniping;

        if (activeInThirdPersonOnly) activeInThirdPersonOnly.SetActive(!isSniping);
        if (activeInSniperZoomOnly) activeInSniperZoomOnly.SetActive(isSniping);
    }

    private void PointSniperCameraAtMouse()
    {
        Assert.IsNotNull(sniperZoomVirtualCamera.LookAt);
        
        var pov = sniperZoomVirtualCamera.GetCinemachineComponent<CinemachinePOV>();
        Vector3 eulerAngles = hardLookAtMouseSniperCamera.transform.eulerAngles;

        pov.m_VerticalAxis.Value = WrapEulerAngle(eulerAngles.x, pov.m_VerticalAxis.m_MinValue, pov.m_VerticalAxis.m_MaxValue);
        pov.m_HorizontalAxis.Value = WrapEulerAngle(eulerAngles.y, pov.m_HorizontalAxis.m_MinValue, pov.m_HorizontalAxis.m_MaxValue);
    }

    private void PointPrimaryCameraAtMouse()
    {        
        CinemachineFreeLook cam = primaryVirtualCamera;
        // OPTIMIZATION Cache this.
        float maxAngle = Mathf.Atan(cam.m_Orbits.Max(o => o.m_Height / o.m_Radius)) * Mathf.Rad2Deg;
        float minAngle = Mathf.Atan(cam.m_Orbits.Min(o => o.m_Height / o.m_Radius)) * Mathf.Rad2Deg;

        Vector3 eulerAngles = hardLookAtMousePrimaryCamera.transform.eulerAngles;

        // TODO Maybe do binary search with cam.GetLocalPositionForCameraFromInput(t) to find the right t for a given angle.
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
        return Mathf.LerpUnclamped(bMin, bMax, (value - aMin) / (aMax - aMin));
    }
}