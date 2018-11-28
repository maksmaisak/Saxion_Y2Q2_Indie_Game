using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] int damage = 1;
    
    private void OnCollisionEnter(Collision other)
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
}