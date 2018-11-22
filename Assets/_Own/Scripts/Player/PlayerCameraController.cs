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
       
        go = new GameObject("HardLookAtMouse");
        go.transform.SetParent(primaryVirtualCamera.transform, worldPositionStays: false);
        hardLookAtMousePrimaryCamera = go.AddComponent<CinemachineVirtualCamera>();
        hardLookAtMousePrimaryCamera.Priority = -1;
        hardLookAtMousePrimaryCamera.m_StandbyUpdate = CinemachineVirtualCameraBase.StandbyUpdateMode.Always;
        hardLookAtMousePrimaryCamera.LookAt = mouse;
        hardLookAtMousePrimaryCamera.AddCinemachineComponent<CinemachineHardLookAt>();
        //go.AddComponent<RotationConstraint>().AddSource(new ConstraintSource());

        currentFov = primaryVirtualCamera.m_Lens.FieldOfView;
        isSniping = currentFov < primaryMinFov;

        sniperZoomVirtualCamera.LookAt = mouse;
        
        renderers = GetComponentsInChildren<Renderer>();
    }

    void Update()
    {
        float scrollAmount = Input.GetAxis("Mouse ScrollWheel");
        float zoomAmount = scrollAmount * zoomSpeed;

        //Vector3 origin = primaryVirtualCamera.Follow.transform.position;
        //Vector3 cameraPos = primaryVirtualCamera.State.FinalPosition;
        //Debug.DrawRay(origin, cameraPos - origin, Color.blue);

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
        }
        else
        {
            currentFov = Mathf.Clamp(currentFov, primaryMinFov, primaryMaxFov);
        }
        
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

        /*Vector3 toTarget = sniperZoomVirtualCamera.LookAt.position - sniperZoomVirtualCamera.transform.position;
        var rotation = Quaternion.LookRotation(toTarget.normalized);
        Vector3 eulerAngles = rotation.eulerAngles;
        var pov = sniperZoomVirtualCamera.GetCinemachineComponent<CinemachinePOV>();

        pov.m_VerticalAxis.Value = WrapEulerAngle(eulerAngles.x, pov.m_VerticalAxis.m_MinValue, pov.m_VerticalAxis.m_MaxValue);
        pov.m_HorizontalAxis.Value = WrapEulerAngle(eulerAngles.y, pov.m_HorizontalAxis.m_MinValue, pov.m_HorizontalAxis.m_MaxValue);
        
        float discrepancy = Quaternion.Angle(rotation, Quaternion.Euler(pov.m_VerticalAxis.Value, pov.m_HorizontalAxis.Value, 0f));
        Assert.IsTrue(discrepancy < 0.01f);*/
    }

    private void PointPrimaryCameraAtMouse()
    {
        Assert.IsNotNull(sniperZoomVirtualCamera.LookAt);
        
        CinemachineFreeLook cam = primaryVirtualCamera;
        // OPTIMIZATION Cache this.
        float maxAngle = Mathf.Atan(cam.m_Orbits.Max(o => o.m_Height / o.m_Radius)) * Mathf.Rad2Deg;
        float minAngle = Mathf.Atan(cam.m_Orbits.Min(o => o.m_Height / o.m_Radius)) * Mathf.Rad2Deg;
        
        for (int i = 0; i < 100; ++i)
        {
            //var rotation = Quaternion.LookRotation(mouse.position - primaryVirtualCamera.State.FinalPosition);
            //Vector3 eulerAngles = rotation.eulerAngles;
            Vector3 eulerAngles = hardLookAtMousePrimaryCamera.transform.eulerAngles;

            float yAxisAngle = WrapEulerAngle(eulerAngles.x, -90f, 90f);
            float yAxisValue = Remap(minAngle, maxAngle, cam.m_YAxis.m_MinValue, cam.m_YAxis.m_MaxValue, yAxisAngle);
            //Debug.Log(yAxisValue);
            cam.m_YAxis.Value = Mathf.Clamp01(yAxisValue);

            cam.m_XAxis.Value = WrapEulerAngle(eulerAngles.y, cam.m_XAxis.m_MinValue, cam.m_XAxis.m_MaxValue);
            
            cam.InternalUpdateCameraState(Vector3.up, Time.deltaTime);
            hardLookAtMousePrimaryCamera.InternalUpdateCameraState(Vector3.up, Time.deltaTime);
            //Debug.Log(cam.m_XAxis.Value);
        }
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