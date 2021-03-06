using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Bullet : MonoBehaviour
{
    [SerializeField] int damage = 1;
    [SerializeField] string ignoreTag = string.Empty;
    [Header("Force applied if target was killed")]
    [SerializeField] float hitImpulseMultiplier = 1f;
    [Space] 
    [SerializeField] UnityEvent OnHit;

    private bool didHit = false;
    
    private void OnCollisionEnter(Collision collision)
    {
        if (didHit)
            return;

        didHit = true;
        
        if (string.IsNullOrWhiteSpace(ignoreTag) || !collision.gameObject.CompareTag(ignoreTag))
        {
            var health = collision.gameObject.GetComponentInChildren<Health>();
            if (!health) health = collision.gameObject.GetComponentInParent<Health>();
            if (health)
            {
                this.DoNextFrame(() =>
                {
                    DealDamage(collision, health);
                    Destroy(gameObject);
                    OnHit.Invoke();
                });
                return;
            }
        }
        
        OnHit.Invoke();
        Destroy(gameObject);
    }

    private void DealDamage(Collision collision, Health health)
    {
        bool wasAlive = health.isAlive;
        health.DealDamage(damage);
        bool didDie = wasAlive && health.isDead;
        if (!didDie) return;
        
        var rb = collision.gameObject.GetComponentInParent<Rigidbody>();
        if (rb == null) return;
        
        //Debug.Log($"Bullet killed target. Collision: relativeVelocity: {collision.relativeVelocity}, impulse: {collision.impulse}");
        
        rb.AddForce(-collision.impulse * hitImpulseMultiplier, ForceMode.Impulse);
    }
}