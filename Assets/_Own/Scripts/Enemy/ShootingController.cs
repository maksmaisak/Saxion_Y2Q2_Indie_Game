using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

/// A ballistics-based projectile shooting helper.
[Serializable]
public class ShootingController
{
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] float muzzleSpeed = 100f;
    [SerializeField] float spawnPointOffset = 1.2f;
    [Space]
    [SerializeField] LayerMask obstacleDetectionLayerMask;
    [SerializeField] float sphereCastRadius = 0.1f;
    [SerializeField] Transform projectileOrigin;

    private GameObject gameObject;
    private bool isInitialized;

    public void Initialize(GameObject gameObject, Transform bulletOriginTransform = null)
    {
        this.gameObject = gameObject;
        
        if (!bulletOriginTransform)
        {
            projectileOrigin = projectileOrigin ? projectileOrigin : gameObject.transform;
        }
        else
        {
            projectileOrigin = bulletOriginTransform;
        }
        
        isInitialized = true;
    }
    
    public bool IsClearPath(GameObject target)
    {
        Assert.IsTrue(isInitialized);
        
        Vector3? startVelocity = Ballistics.GetStartVelocity(
            projectileOrigin.position,
            target.transform.position,
            muzzleSpeed
        );

        if (!startVelocity.HasValue) return false;

        Vector3 delta = target.transform.position - projectileOrigin.position;

        RaycastHit hit;
        bool didHit = Physics.SphereCast(
            origin: projectileOrigin.position,
            radius: sphereCastRadius,
            direction: delta.normalized,
            hitInfo: out hit,
            maxDistance: delta.magnitude,
            layerMask: obstacleDetectionLayerMask & ~(1 << target.layer),
            queryTriggerInteraction: QueryTriggerInteraction.Ignore
        );

        return !didHit || hit.collider.gameObject == gameObject;
    }

    /// Returns true if successful
    public bool ShootAt(GameObject target)
    {
        Assert.IsTrue(isInitialized);
        
        Vector3 delta = target.transform.position - projectileOrigin.position;
        Vector3 direction = delta.normalized;

        Vector3 projectileSpawnPosition = projectileOrigin.position + direction * spawnPointOffset;

        Vector3? startVelocity = Ballistics.GetStartVelocity(
            start: projectileSpawnPosition,
            target: target.transform.position,
            muzzleSpeed: muzzleSpeed
        );

        if (!startVelocity.HasValue) return false;

        Shoot(
            position: projectileSpawnPosition,
            startVelocity: startVelocity.Value
        );

        return true;
    }

    private void Shoot(Vector3 position, Vector3 startVelocity)
    {
        Assert.IsNotNull(bulletPrefab);
        
        Object.Instantiate(bulletPrefab, position, Quaternion.identity)
            .GetComponent<Rigidbody>()
            .AddForce(startVelocity, ForceMode.VelocityChange);
    }
}
