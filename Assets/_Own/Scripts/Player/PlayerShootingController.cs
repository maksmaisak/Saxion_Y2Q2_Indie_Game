using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine.Utility;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEditor;
using Random = UnityEngine.Random;

[Serializable]
class SnapShootingImprecision
{
    public float angle = 5f;
}

public class PlayerShootingController : MyBehaviour
{
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform bulletSpawnLocation;
    [SerializeField] Transform aimingTarget;
    [SerializeField] Animator playerAnimator;
    [SerializeField] float reloadInterval = 1f;
    [SerializeField] float bulletSpeed = 20f;
    [SerializeField] SnapShootingImprecision snapShootingImprecisionStanding;
    [SerializeField] SnapShootingImprecision snapShootingImprecisionCrouching;
    
    [Header("Enemy shot hearing")]
    [SerializeField] float shotDistractionPriority = 2.0f;
    [SerializeField] float shotHearingRange = 30f;

    [SerializeField] UnityEvent OnShoot;
    
    private float timeWhenCanShoot;
    private PlayerCameraController cameraController;
    
#if UNITY_EDITOR
    [CustomEditor(typeof(PlayerShootingController))]
    public class DistractionOnDeathEditor : Editor
    {
        void OnSceneGUI()
        {
            var playerShootingController = (PlayerShootingController)target;
            var shotOrigin = playerShootingController.bulletSpawnLocation;
            if (!shotOrigin) shotOrigin = playerShootingController.transform;
            
            EditorUtils.UpdateHearingRadiusWithHandles(
                playerShootingController, 
                shotOrigin, 
                ref playerShootingController.shotHearingRange
            );
        }
    }
#endif
    
    void Start()
    {
        CursorHelper.SetLock(true);
        
        if (!aimingTarget) aimingTarget = FindObjectOfType<AimingTarget>().transform;
        Assert.IsNotNull(aimingTarget);
                
        if (!bulletSpawnLocation) bulletSpawnLocation = transform;
        if (!cameraController) cameraController = GetComponent<PlayerCameraController>();
        if (!playerAnimator) playerAnimator = GetComponentInChildren<Animator>();

        GetComponent<Health>().OnDeath += sender => enabled = false;
    }

    void Update()
    {
        if (!aimingTarget) return;
        
        if (cameraController.isSniping && Input.GetMouseButtonDown(0) && CanShoot())
        {
            Shoot();
            OnShoot.Invoke();
        }
    }
    
    private void Shoot()
    {
        Vector3 targetPosition = aimingTarget.transform.position;

        var spawnLocation = (cameraController && cameraController.isSniping) ? Camera.main.transform : bulletSpawnLocation;
        Vector3 position = spawnLocation.position;
        Vector3 toTarget = targetPosition - position;

        if (cameraController && !cameraController.isSniping) toTarget = ApplyImprecision(toTarget);

        Vector3 bulletForward = toTarget.normalized;
        var bullet = Instantiate(bulletPrefab, position, Quaternion.LookRotation(bulletForward));
        bullet.GetComponent<Rigidbody>().velocity = bulletForward * bulletSpeed;

        new Distraction(transform.position, shotDistractionPriority, shotHearingRange).PostEvent();

        timeWhenCanShoot = Time.time + reloadInterval;
    }

    private Vector3 ApplyImprecision(Vector3 toTarget)
    {
        SnapShootingImprecision snapShootingImprecision;

        snapShootingImprecision = IsCrouching() ?
            snapShootingImprecisionCrouching :
            snapShootingImprecisionStanding;

        float angleRadians = snapShootingImprecision.angle * Mathf.Deg2Rad;
        float distanceToTarget = toTarget.magnitude;
        float radius = Mathf.Sin(angleRadians) * distanceToTarget;

        // TODO generate from a unit circle orthogonal circle instead.
        // Because this has cases where the point from the sphere goes to zero.
        Vector3 offset = Random.insideUnitSphere.ProjectOntoPlane(toTarget.normalized).normalized * Random.Range(0f, radius);
        return toTarget + offset;
    }

    public bool IsCrouching()
    {
        return playerAnimator && playerAnimator.GetBool("Crouch");
    }

    private bool CanShoot() => Time.time >= timeWhenCanShoot;
    private float TimeTillCanShoot() => timeWhenCanShoot - Time.time;
    
    private static bool Shorter(Vector3 vector, float maxMagnitude) => vector.sqrMagnitude < maxMagnitude * maxMagnitude;
}