using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine.Utility;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

[Serializable]
struct SnapImprecision
{
    public static readonly SnapImprecision Default = new SnapImprecision
    {
        worldspaceRadius = 2f,
        maxAngle = 20f
    };
    
    public float worldspaceRadius;
    public float maxAngle;
}

public class PlayerShootingController : MonoBehaviour
{
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform bulletSpawnLocation;
    [SerializeField] Transform aimingTarget;
    [SerializeField] float reloadInterval = 1f;
    [SerializeField] float bulletSpeed = 20f;
    [SerializeField] SnapImprecision snapShootingImprecision = SnapImprecision.Default;

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
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        CursorHelper.SetLock(!pauseStatus);
    }

    void Update()
    {
        if (!aimingTarget) return;
        
        if (CanShoot() && Input.GetMouseButton(0))
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
        float radius = snapShootingImprecision.worldspaceRadius;
        float distanceToTarget = toTarget.magnitude;

        float maxAngleRadians = snapShootingImprecision.maxAngle * Mathf.Deg2Rad;
        if (Mathf.Asin(radius / distanceToTarget) > maxAngleRadians)
        {
            radius = Mathf.Sin(maxAngleRadians) * distanceToTarget;
        }

        // TODO generate from a unit circle orthogonal circle instead.
        // Because this has cases where the point from the sphere goes to zero.
        Vector3 offset = Random.insideUnitSphere.ProjectOntoPlane(toTarget.normalized).normalized *
                       Random.Range(0f, radius);
        return toTarget + offset;
    }

    private bool CanShoot() => Time.time >= timeWhenCanShoot;
    private float TimeTillCanShoot() => timeWhenCanShoot - Time.time;
    
    private static bool Shorter(Vector3 vector, float maxMagnitude) => vector.sqrMagnitude < maxMagnitude * maxMagnitude;
}