using System;
using UnityEngine;
using UnityEngine.Events;

public class Bullet : MonoBehaviour
{
    [SerializeField] int damage = 1;
    [SerializeField] string ignoreTag = string.Empty;
    [Space] 
    [SerializeField] UnityEvent OnHit;
            
    private void OnCollisionEnter(Collision other)
    {
        if (ignoreTag == string.Empty || !other.gameObject.CompareTag(ignoreTag))
        {
            var health = other.gameObject.GetComponentInChildren<Health>();
            if (!health) health = other.gameObject.GetComponentInParent<Health>();
            if (health)
            {
                this.DoNextFrame(() =>
                {
                    health.DealDamage(damage);
                    Destroy(gameObject);
                });
            }
        }

        OnHit.Invoke();
        Destroy(gameObject);
    }
}