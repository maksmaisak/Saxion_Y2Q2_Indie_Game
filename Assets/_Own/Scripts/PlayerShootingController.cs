using System.Collections;
using System.Collections.Generic;
using Cinemachine.Utility;
using UnityEngine;
using UnityEngine.Assertions;
using DG.Tweening;
using DG.Tweening.Core.Easing;
using UnityEngine.Serialization;

public class PlayerShootingController : MonoBehaviour
{
    [SerializeField] Transform aimingTarget;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform bulletSpawnLocation;
    [SerializeField] float reloadInterval = 1f;
    [SerializeField] float bulletSpeed = 20f;
    [Space]
    [SerializeField] FovInfo currentAimingFov = new FovInfo {maxAngle = 15, maxDistance = 30f};
    [SerializeField] FovInfo noAimingFov = new FovInfo {maxAngle = 15f, maxDistance = 30f};
    [SerializeField] FovDisplay fovDisplay;
    [SerializeField] float aimingAngleDecreasePerSecond = 10f;
    [SerializeField] float aimingAngleIncreasePerSecond = 40f;
    [SerializeField] float maxMouseMovementForAiming = 1f;
    [SerializeField] float maxOwnMovementForAiming = 1f;

    private float timeWhenCanShoot;
    private Vector3 previousAimingTargetPosition;
    private Vector3 previousOwnPosition;

    void Start()
    {
        Assert.IsNotNull(aimingTarget);
    }

    void Update()
    {        
        UpdateFovDisplay();
        Shoot();
        
        previousAimingTargetPosition = aimingTarget.position;
        previousOwnPosition = transform.position;
    }
    
    void UpdateFovDisplay()
    {
        if (!fovDisplay) return;

        // direction
        fovDisplay.transform.forward = (aimingTarget.position - transform.position).normalized;

        // fade
        float timeTillCanShoot = TimeTillCanShoot();
        if (timeTillCanShoot > 0f)
        {
            // The last two parameters are not need for this function.
            fovDisplay.fade = EaseManager.ToEaseFunction(Ease.InExpo)(reloadInterval - timeTillCanShoot, reloadInterval, 0, 0);
        }
        else
        {
            fovDisplay.fade = 1f;
        }

        // angle
        if (!CanShoot())
        {
            currentAimingFov.maxAngle = noAimingFov.maxAngle;
        }
        else if (!IsAiming())
        {
            currentAimingFov.maxAngle += aimingAngleIncreasePerSecond * Time.deltaTime;
        }
        else
        {
            currentAimingFov.maxAngle -= aimingAngleDecreasePerSecond * Time.deltaTime;
            if (currentAimingFov.maxAngle < 0f) currentAimingFov.maxAngle = 0.01f;
        }
        currentAimingFov.maxAngle = Mathf.Clamp(currentAimingFov.maxAngle, 1f, noAimingFov.maxAngle);
        fovDisplay.fov = currentAimingFov;
    }

    void Shoot()
    {
        if (!aimingTarget) return;
        if (!CanShoot()) return;
        if (!Input.GetMouseButton(0)) return;

        var spawnLocation = bulletSpawnLocation ? bulletSpawnLocation : transform;
        Vector3 position = spawnLocation.position;
        Vector3 toTarget = (aimingTarget.position - position).ProjectOntoPlane(Vector3.up).normalized;
        var imprecisionOffset = Quaternion.AngleAxis(Random.Range(-currentAimingFov.maxAngle * 0.5f, currentAimingFov.maxAngle * 0.5f), Vector3.up);
        Vector3 bulletForward = imprecisionOffset * toTarget;
        
        var bullet = Instantiate(bulletPrefab, position, Quaternion.LookRotation(bulletForward));
        bullet.GetComponent<Rigidbody>().velocity = bulletForward * bulletSpeed;
        
        timeWhenCanShoot = Time.time + reloadInterval;
    }
    
    bool CanShoot() => Time.time >= timeWhenCanShoot;
    float TimeTillCanShoot() => timeWhenCanShoot - Time.time;

    bool IsAiming()
    {
        Vector3 aimingTargetPosition = aimingTarget.transform.position;
        Vector3 ownPosition = transform.position;
        
        Vector3 deltaAimingTarget = aimingTargetPosition - previousAimingTargetPosition;
        deltaAimingTarget -= Vector3.Project(deltaAimingTarget, aimingTargetPosition - ownPosition);
        if (deltaAimingTarget.sqrMagnitude >= Sqr(maxMouseMovementForAiming * Time.deltaTime)) return false;

        Vector3 deltaOwn = transform.position - previousOwnPosition;
        if (deltaOwn.sqrMagnitude >= Sqr(maxOwnMovementForAiming * Time.deltaTime)) return false;

        return true;
    }

    static float Sqr(float value) => value * value;
}