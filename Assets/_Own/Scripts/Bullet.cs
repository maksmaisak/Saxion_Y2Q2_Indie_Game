using System;
using UnityEngine;
using UnityEngine.Events;

public class Bullet : MonoBehaviour
{
    [SerializeField] int damage = 1;
    [SerializeField] string ignoreTag = string.Empty;
    [SerializeField] float hitImpulseMultiplier = 1f;
    [Space] 
    [SerializeField] UnityEvent OnHit;
            
    private void OnCollisionEnter(Collision collision)
    {
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
        
        rb.AddForce(-collision.impulse * hitImpulseMultiplier, ForceMode.Impulse);
    }
}