using UnityEngine;

public class EnemyDeath : MonoBehaviour
{
    [SerializeField] Transform headshotBone;
    
    private Transform lastBoneHit;
    private bool wasHitThisFrame;

    void Start()
    {
        GetComponent<Health>().OnDeath += OnDeath;

        foreach (Collider collider in GetComponentsInChildren<Collider>())
        {
            if (collider.isTrigger) continue;
            var bulletCollisionDetector = collider.GetComponent<BulletCollisionDetector>();
            if (!bulletCollisionDetector)
            {
                bulletCollisionDetector = collider.gameObject.AddComponent<BulletCollisionDetector>();
            }

            bulletCollisionDetector.OnBulletHit += OnBulletHit;
        }
    }

    void OnBulletHit(Collision collision)
    {
        if (wasHitThisFrame) return;
        
        Collider ownCollider = collision.contacts[0].thisCollider;
        lastBoneHit = ownCollider.transform;

        wasHitThisFrame = true;
        this.DoNextFrame(() => wasHitThisFrame = false);
    }
    
    private void OnDeath(Health sender)
    {
        new OnEnemyDied(WasHeadshot()).PostEvent();
    }

    private bool WasHeadshot()
    {
        if (!lastBoneHit) return false;
        if (!headshotBone) return false;

        return lastBoneHit.IsChildOf(headshotBone);
    }
}