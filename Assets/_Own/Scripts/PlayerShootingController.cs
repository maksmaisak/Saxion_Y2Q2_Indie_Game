using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerShootingController : MonoBehaviour
{
    [SerializeField] Transform aimingTarget;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform bulletSpawnLocation;
    [SerializeField] float reloadInterval = 1f;
    [SerializeField] float bulletSpeed = 20f;

    private float timeWhenCanShoot;

    void Start()
    {
        Assert.IsNotNull(aimingTarget);
    }

    void Update()
    {
        if (!aimingTarget) return;
        if (!CanShoot()) return;
        if (!Input.GetMouseButton(0)) return;

        var spawnLocation = bulletSpawnLocation ?? transform;
        Vector3 position = spawnLocation.position;
        Vector3 toTarget = (aimingTarget.position - position).normalized;
        var bullet = Instantiate(bulletPrefab, position, Quaternion.LookRotation(toTarget));

        bullet.GetComponent<Rigidbody>().velocity = toTarget * bulletSpeed;
        
        timeWhenCanShoot = Time.unscaledTime + reloadInterval;
    }

    bool CanShoot()
    {
        return Time.unscaledTime >= timeWhenCanShoot;
    }
}