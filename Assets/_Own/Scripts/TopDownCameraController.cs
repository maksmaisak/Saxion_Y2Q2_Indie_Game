using UnityEngine;
using UnityEngine.Assertions;
using Cinemachine;

public class TopDownCameraController : MonoBehaviour
{
   [SerializeField] CinemachineVirtualCamera virtualCamera;
   [SerializeField] float zoomSpeed = 1f;
   [SerializeField] float minCameraDistance = 10f;
   [SerializeField] float maxCameraDistance = 30f;

   private CinemachineFramingTransposer framer;

   void Start()
   {
      Assert.IsNotNull(virtualCamera);
      framer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
   }

   void Update()
   {
      if (!framer) return;

      float scrollAmount = Input.GetAxis("Mouse ScrollWheel");
      float cameraDistance = framer.m_CameraDistance - scrollAmount * zoomSpeed * Time.deltaTime;
      framer.m_CameraDistance = Mathf.Clamp(cameraDistance, minCameraDistance, maxCameraDistance);
   }
}