using System;
using System.Linq;
using UnityEngine;
using Cinemachine;
using UnityEngine.Assertions;

[Serializable]
struct CameraNoiseConfig
{
    public static readonly CameraNoiseConfig Default = new CameraNoiseConfig
    {
        amplitudeGain = 1f, 
        frequencyGain = 1f
    };
    
    [Tooltip("Gain to apply to the amplitudes defined in the NoiseSettings asset.  1 is normal.  Setting this to 0 completely mutes the noise.")]
    public float amplitudeGain;
    [Tooltip("Scale factor to apply to the frequencies defined in the NoiseSettings asset.  1 is normal.  Larger magnitudes will make the noise shake more rapidly.")]
    public float frequencyGain;
}

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] CinemachineFreeLook primaryVirtualCamera;
    [SerializeField] CinemachineVirtualCamera sniperZoomVirtualCamera;
    [SerializeField] Transform mouse;
    [SerializeField] Animator playerAnimator;
    
    [Header("Zoom & Field Of View")]
    [SerializeField] float zoomSpeed = 10f;
    [SerializeField] float primaryMaxFov = 40f;
    [SerializeField] float primaryMinFov = 40f;
    [SerializeField] float sniperMaxFov = 10f;
    [SerializeField] float sniperMinFov = 5f;
    [SerializeField] bool invertScroll = false;
    [SerializeField] float currentFov;

    [Header("Sniper camera shake")]
    [SerializeField] private CameraNoiseConfig sniperCameraShakeStanding  = CameraNoiseConfig.Default;
    [SerializeField] private CameraNoiseConfig sniperCameraShakeCrouching = CameraNoiseConfig.Default;

    public bool isSniping { get; private set; }
    private CinemachineVirtualCamera hardLookAtMouseSniperCamera;
    private CinemachineVirtualCamera hardLookAtMousePrimaryCamera;
    private Renderer[] renderers;
    
    private GameObject activeInThirdPersonOnly;
    private GameObject activeInSniperZoomOnly;
        
    void Start()
    {
        Assert.IsNotNull(primaryVirtualCamera);
        Assert.IsNotNull(sniperZoomVirtualCamera);
        
        if (!mouse) mouse = FindObjectOfType<AimingTarget>().transform;
        Assert.IsNotNull(mouse);
        sniperZoomVirtualCamera.LookAt = mouse;

        hardLookAtMouseSniperCamera = MakeHardLookAtMouseSniperCamera();
        hardLookAtMousePrimaryCamera = MakeHardLookAtMousePrimaryCamera();
        currentFov = primaryVirtualCamera.m_Lens.FieldOfView;
        isSniping = currentFov < primaryMinFov;
        
        renderers = GetComponentsInChildren<Renderer>();
        if (!playerAnimator) playerAnimator = GetComponentInChildren<Animator>();
        GetComponent<Health>().OnDeath += OnDeath;
        
        var levelCanvas = LevelCanvas.instance;
        Assert.IsNotNull(levelCanvas, "Level canvas not found! Put in the canvas prefab if it's not there.");
        activeInThirdPersonOnly = levelCanvas.activeInThirdPersonOnly;
        activeInSniperZoomOnly  = levelCanvas.activeInSniperZoomOnly;
    }

    void Update()
    {
        UpdateZoom();
        
        UpdateCameras();
        
        foreach (Renderer r in renderers) r.enabled = !isSniping;

        if (activeInThirdPersonOnly) activeInThirdPersonOnly.SetActive(!isSniping);
        if (activeInSniperZoomOnly) activeInSniperZoomOnly.SetActive(isSniping);

        UpdateSniperCameraShake();
    }

    private void OnDeath(Health health)
    {
        currentFov = primaryMaxFov;
        UpdateCameras();
        
        if (activeInThirdPersonOnly) activeInThirdPersonOnly.SetActive(false);
        if (activeInSniperZoomOnly) activeInSniperZoomOnly.SetActive(false);
        
        enabled = false;
    }
    
    private void UpdateZoom()
    {
        float scrollAmount = Input.GetAxis("Mouse ScrollWheel");
        float zoomAmount = -scrollAmount * zoomSpeed;
        if (invertScroll) zoomAmount = -zoomAmount;

        currentFov += zoomAmount;
    }

    private void UpdateCameras()
    {
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
    }

    private void UpdateSniperCameraShake()
    {
        bool isCrouching = playerAnimator && playerAnimator.GetBool("Crouch");
        var noise = sniperZoomVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        if (isCrouching)
        {
            noise.m_AmplitudeGain = sniperCameraShakeCrouching.amplitudeGain;
            noise.m_FrequencyGain = sniperCameraShakeCrouching.frequencyGain;
        }
        else
        {
            noise.m_AmplitudeGain = sniperCameraShakeStanding.amplitudeGain;
            noise.m_FrequencyGain = sniperCameraShakeStanding.frequencyGain;
        }
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

    private CinemachineVirtualCamera MakeHardLookAtMouseSniperCamera()
    {
        GameObject go = Instantiate(sniperZoomVirtualCamera.gameObject, sniperZoomVirtualCamera.transform.parent);
        go.name = "CM HardLookAtMouseSniperCamera";
        var cam = go.GetComponent<CinemachineVirtualCamera>();
        
        cam.Priority = -1;
        cam.m_StandbyUpdate = CinemachineVirtualCameraBase.StandbyUpdateMode.Always;
        cam.AddCinemachineComponent<CinemachineHardLookAt>();
        cam.DestroyCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        return cam;
    }

    private CinemachineVirtualCamera MakeHardLookAtMousePrimaryCamera()
    {
        var go = new GameObject("CM HardLookAtMouse");
        go.transform.SetParent(primaryVirtualCamera.transform, worldPositionStays: false);
        var cam = go.AddComponent<CinemachineVirtualCamera>();
        
        cam.Priority = -1;
        cam.m_StandbyUpdate = CinemachineVirtualCameraBase.StandbyUpdateMode.Always;
        cam.LookAt = mouse;
        cam.AddCinemachineComponent<CinemachineHardLookAt>();

        return cam;
    }

    private static float WrapEulerAngle(float angle, float min, float max)
    {
        float delta = max - min;
        while (angle < min) angle += delta;
        while (angle > max) angle -= delta;
        return angle;
    }
    
    /// Value in range (aMin, aMax) -> value in range (bMin, bMax)
    private static float Remap(float aMin, float aMax, float bMin, float bMax, float value) 
    {
        return Mathf.LerpUnclamped(bMin, bMax, (value - aMin) / (aMax - aMin));
    }
}