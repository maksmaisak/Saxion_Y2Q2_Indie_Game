using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class PoisonCloud : MonoBehaviour
{
    [SerializeField] float duration = 10f;
    [SerializeField] int damage = 10;
    [SerializeField] float damageInterval = 1f;
    [SerializeField] LayerMask canDamageLayer = ~0;
    [SerializeField] string ignoreTag = string.Empty;

    private readonly Dictionary<GameObject, Coroutine> damageCoroutines = new Dictionary<GameObject, Coroutine>();

    void Start()
    {
        Assert.IsTrue(
            gameObject.GetComponentsInChildren<Collider>().Any(c => c.isTrigger), 
            "No trigger collider found under PoisonCloud. Can't damage anyone without a trigger."
        );
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject go = other.gameObject;
        if (damageCoroutines.ContainsKey(go)) return;
        
        var health = go.GetComponentInChildren<Health>();
        if (!health) health = go.GetComponentInParent<Health>();
        if (!health) return;
        
        Coroutine damageCoroutine = StartCoroutine(DamageCoroutine(health));
        damageCoroutines.Add(go, damageCoroutine);
    }

    private void OnTriggerExit(Collider other)
    {
        GameObject go = other.gameObject;
        
        Coroutine damageCoroutine;
        if (!damageCoroutines.TryGetValue(go, out damageCoroutine))
            return;
        
        StopCoroutine(damageCoroutine);
        damageCoroutines.Remove(go);
    }

    private IEnumerator DamageCoroutine(Health health)
    {
        while (enabled)
        {
            health.DealDamage(damage);
            yield return new WaitForSeconds(damageInterval);
        }
    }
}