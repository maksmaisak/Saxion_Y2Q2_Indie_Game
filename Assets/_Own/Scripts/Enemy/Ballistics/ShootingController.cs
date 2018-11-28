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

    /// Returns true if successful
    public bool ShootAt(GameObject target)
    {
        Assert.IsTrue(isInitialized);
        
        Vector3 delta = target.transform.position - projectileOrigin.position;
        Vector3 direction = delta.normalized;
        Vector3 projectileSpawnPosition = projectileOrigin.position + direction * spawnPointOffset;

        BallisticTrajectory? trajectory = GetTrajectoryTo(target);
        
        if (!trajectory.HasValue) return false;

        Shoot(
            position: projectileSpawnPosition,
            startVelocity: trajectory.Value.startVelocity
        );

        return true;
    }
    
    public BallisticTrajectory? GetTrajectoryTo(GameObject target)
    {
        Ballistics.TrajectoryPair? trajectories = Ballistics.GetLowAndHighTrajectories(
            projectileOrigin.position, 
            target.transform.position,
            muzzleSpeed
        );
        if (!trajectories.HasValue) return null;
        
        LayerMask layerMask = obstacleDetectionLayerMask & ~(1 << target.layer);

        if (trajectories.Value.low.CheckIsClear(layerMask, target)) 
            return trajectories.Value.low;
        
        if (trajectories.Value.high.CheckIsClear(layerMask, target)) 
            return trajectories.Value.high;

        return null;
    }

    private void Shoot(Vector3 position, Vector3 startVelocity)
    {
        Assert.IsNotNull(bulletPrefab);
        
        Object.Instantiate(bulletPrefab, position, Quaternion.identity)
            .GetComponent<Rigidbody>()
            .AddForce(startVelocity, ForceMode.VelocityChange);
    }
}
