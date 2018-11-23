﻿using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine.Utility;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

[Serializable]
class SnapShootingImprecision
{
    public float worldspaceRadius = 1f;
    public float maxAngle = 10f;
}

public class PlayerShootingController : MonoBehaviour
{
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform bulletSpawnLocation;
    [SerializeField] Transform aimingTarget;
    [SerializeField] Animator playerAnimator;
    [SerializeField] float reloadInterval = 1f;
    [SerializeField] float bulletSpeed = 20f;
    [SerializeField] SnapShootingImprecision snapShootingImprecisionStanding;
    [SerializeField] SnapShootingImprecision snapShootingImprecisionCrouching;

    private float timeWhenCanShoot;
    private PlayerCameraController cameraController;
    
    void Start()
    {
        if (!aimingTarget)
        {
            aimingTarget = FindObjectOfType<AimingTarget>().transform;
        }
        Assert.IsNotNull(aimingTarget);

        if (!cameraController) cameraController = GetComponent<PlayerCameraController>();
        if (!playerAnimator) playerAnimator = GetComponentInChildren<Animator>();
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        CursorHelper.SetLock(!pauseStatus);
    }

    void Update()
    {
        if (!aimingTarget) return;
        
        if (CanShoot() && Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        Vector3 targetPosition = aimingTarget.transform.position;
        
        var spawnLocation = bulletSpawnLocation ? bulletSpawnLocation : transform;
        Vector3 position = spawnLocation.position;
        Vector3 toTarget = targetPosition - position;

        if (cameraController && !cameraController.isSniping) toTarget = ApplyImprecision(toTarget);

        Vector3 bulletForward = toTarget.normalized;
        var bullet = Instantiate(bulletPrefab, position, Quaternion.LookRotation(bulletForward));
        bullet.GetComponent<Rigidbody>().velocity = bulletForward * bulletSpeed;
        
        timeWhenCanShoot = Time.time + reloadInterval;
    }

    private Vector3 ApplyImprecision(Vector3 toTarget)
    {
        SnapShootingImprecision snapShootingImprecision;

        snapShootingImprecision = GetIsCrouching() ? 
            snapShootingImprecisionCrouching : 
            snapShootingImprecisionStanding;
        
        float radius = snapShootingImprecision.worldspaceRadius;
        float distanceToTarget = toTarget.magnitude;

        float maxAngleRadians = snapShootingImprecision.maxAngle * Mathf.Deg2Rad;
        if (Mathf.Asin(radius / distanceToTarget) > maxAngleRadians)
        {
            radius = Mathf.Sin(maxAngleRadians) * distanceToTarget;
        }

        // TODO generate from a unit circle orthogonal circle instead.
        // Because this has cases where the point from the sphere goes to zero.
        Vector3 offset = Random.insideUnitSphere.ProjectOntoPlane(toTarget.normalized).normalized * Random.Range(0f, radius);
        return toTarget + offset;
    }

    private bool GetIsCrouching()
    {
        return playerAnimator && playerAnimator.GetBool("Crouch");
    }

    private bool CanShoot() => Time.time >= timeWhenCanShoot;
    private float TimeTillCanShoot() => timeWhenCanShoot - Time.time;
    
    private static bool Shorter(Vector3 vector, float maxMagnitude) => vector.sqrMagnitude < maxMagnitude * maxMagnitude;
}