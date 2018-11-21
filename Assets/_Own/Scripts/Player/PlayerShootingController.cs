using System.Collections;
using System.Collections.Generic;
using Cinemachine.Utility;
using UnityEngine;
using UnityEngine.Assertions;
using DG.Tweening;
using DG.Tweening.Core.Easing;

public class PlayerShootingController : MonoBehaviour
{
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform bulletSpawnLocation;
    [SerializeField] Transform aimingTarget;
    [SerializeField] float reloadInterval = 1f;
    [SerializeField] float bulletSpeed = 20f;

    private float timeWhenCanShoot;
    
    void Start()
    {
        if (!aimingTarget)
        {
            aimingTarget = FindObjectOfType<AimingTarget>().transform;
        }
        Assert.IsNotNull(aimingTarget);
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

        Vector3 bulletForward = toTarget.normalized;
        var bullet = Instantiate(bulletPrefab, position, Quaternion.LookRotation(bulletForward));
        bullet.GetComponent<Rigidbody>().velocity = bulletForward * bulletSpeed;
        
        timeWhenCanShoot = Time.time + reloadInterval;
    }
        
    private bool CanShoot() => Time.time >= timeWhenCanShoot;
    private float TimeTillCanShoot() => timeWhenCanShoot - Time.time;
    
    private static bool Shorter(Vector3 vector, float maxMagnitude) => vector.sqrMagnitude < maxMagnitude * maxMagnitude;
}