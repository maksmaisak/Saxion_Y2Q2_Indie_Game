using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class PoisonCloud : MonoBehaviour
{
    [SerializeField] float duration = 10f;
    [SerializeField] float dissipationDuration = 5f;
    [Tooltip("Can only damage anything after this time after starting.")]
    [SerializeField] float startDelay = 0.5f;
    [Header("Damage")]
    [SerializeField] int damage = 10;
    [SerializeField] float damageInterval = 1f;
    [SerializeField] LayerMask canDamageLayer = ~0;
    [SerializeField] string ignoreTag = string.Empty;
    [Header("Snapping")] 
    [SerializeField] LayerMask snapToSurfaceLayerMask = Physics.DefaultRaycastLayers;
    [SerializeField] float maxSnapDistance = 1f;

    private bool canDamage;
    private readonly Dictionary<GameObject, Coroutine> damageCoroutines = new Dictionary<GameObject, Coroutine>();

    void Start()
    {        
        Assert.IsTrue(
            gameObject.GetComponentsInChildren<Collider>().Any(c => c.isTrigger), 
            "No trigger collider found under PoisonCloud. Can't damage anyone without a trigger."
        );

        SnapToSurface();
        StartCoroutine(DissipateCoroutine());
        
        if (startDelay > 0f)
        {
            SetTriggersEnabled(false);
            this.Delay(startDelay, () => SetTriggersEnabled(true));
        }
    }

    void OnDisable()
    {
        damageCoroutines.Clear();
        StopAllCoroutines();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!enabled) return;
        
        GameObject go = other.gameObject;
        if (!string.IsNullOrWhiteSpace(ignoreTag) && go.CompareTag(ignoreTag)) return;
        if (damageCoroutines.ContainsKey(go)) return;
        
        var health = go.GetComponentInChildren<Health>();
        if (!health) health = go.GetComponentInParent<Health>();
        if (!health) return;
        if (((1 << health.gameObject.layer) & canDamageLayer) == 0) return;
        
        Coroutine damageCoroutine = StartCoroutine(DamageCoroutine(health));
        damageCoroutines.Add(go, damageCoroutine);
    }

    void OnTriggerExit(Collider other)
    {
        GameObject go = other.gameObject;
        
        Coroutine damageCoroutine;
        if (!damageCoroutines.TryGetValue(go, out damageCoroutine))
            return;
        
        StopCoroutine(damageCoroutine);
        damageCoroutines.Remove(go);
    }
    
    private void SnapToSurface()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit;

        bool didHit = Physics.Raycast(
            ray, out hit, maxSnapDistance, 
            snapToSurfaceLayerMask, QueryTriggerInteraction.Ignore
        );
        if (!didHit) return;

        transform.position = hit.point;
        transform.rotation = Quaternion.identity;
    }
    
    private IEnumerator DamageCoroutine(Health health)
    {
        while (enabled)
        {
            health.DealDamage(damage);
            yield return new WaitForSeconds(damageInterval);
        }
    }
    
    private IEnumerator DissipateCoroutine()
    {
        float time = Time.time;
        // Not using a simple WaitForSecond to make it possible to change duration while the cloud is active.
        yield return new WaitUntil(() => Time.time >= time + duration);

        GetComponentInChildren<ParticleSystem>()?.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        time = Time.time;
        yield return new WaitUntil(() => Time.time >= time + dissipationDuration);
        enabled = false;
    }

    private void SetTriggersEnabled(bool isActive = true)
    {
        foreach (Collider trigger in GetComponentsInChildren<Collider>().Where(c => c.isTrigger))
        {
            trigger.enabled = isActive;
        }
    }
}