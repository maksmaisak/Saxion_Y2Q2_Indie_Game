using System;
using UnityEngine;
using UnityEngine.Assertions;

public class BulletCollisionDetector : MonoBehaviour
{
    private const string bulletLayerName = "Bullet";
    private int bulletLayerIndex;

    public Action<Collision> OnBulletHit;
    
    void Awake()
    {
        bulletLayerIndex = LayerMask.NameToLayer(bulletLayerName);
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == bulletLayerIndex)
            OnBulletHit?.Invoke(collision);
    }
}
