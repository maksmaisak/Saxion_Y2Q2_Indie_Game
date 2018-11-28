using System;
using UnityEngine;
using UnityEngine.Assertions;

public class BulletCollisionDetector : MonoBehaviour
{
    private EnemyAI agent;
    private const string bulletLayerName = "Bullet";
    private int bulletLayerIndex;
    
    private void Start()
    {
        agent = GetComponentInParent<EnemyAI>();
        Assert.IsNotNull(agent);

        bulletLayerIndex = LayerMask.NameToLayer(bulletLayerName);
    }
    
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer == bulletLayerIndex)
            agent.SetAttachedObjectHit(gameObject);
    }
}
