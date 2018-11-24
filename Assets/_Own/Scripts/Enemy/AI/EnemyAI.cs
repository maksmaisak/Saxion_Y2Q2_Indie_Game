using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using  Cinemachine.Utility;
using UnityEngine.Assertions;

public enum InvestigateReason
{
    AllyDeath = 1,
    PlayerSeen,
    None,
}

[DisallowMultipleComponent]
[RequireComponent(typeof(NavMeshAgent), typeof(Health))]
public class EnemyAI : MyBehaviour, IEventReceiver<Distraction>
{
    [Header("AI Search Settings")]
    [SerializeField] LayerMask blockingLayerMask = Physics.DefaultRaycastLayers;
    [SerializeField] float maxViewAngle = 60.0f;
    [SerializeField] float maxViewDistance = 20.0f;
    [SerializeField] float disturbanceHearingRadius = 10f;
    [SerializeField] float maxSearchRadius = 5.0f;
    [Space]
    [Header("AI Settings")]
    [SerializeField] float secondsToChase = 2.0f;
    [SerializeField] float secondsToInvestigate = 1.0f;
    [SerializeField] float secondsToRememberPlayer = 2.0f;
    [SerializeField] bool canPatrol = false;
    [Space]
    [Header("AI Movement")]
    public float chaseSpeed = 2.3f;
    public float patrolSpeed = 0.6f;
    public float wanderSpeed = 1.4f;
    public float investigateSpeed = 1.2f;
    public float goBackSpeed = 1f;
    [SerializeField] float stoppingDistanceBeforeLastPlayerPosition = 2f;

    [Header("AI Assignable")]
    [SerializeField] GameObject indicatorPrefab;
    [SerializeField] Transform trackerTransform;

    /********* PUBLIC *********/
    public Health health { get; private set; }

    public FSM<EnemyAI> fsm { get; private set; }
    public NavMeshAgent navMeshAgent { get; private set; }

    public Vector3 spawnPosition { get; private set; }
    public Vector3? lastInvestigatePosition { get; set; }
    public Vector3 lastKnownPlayerPosition { get; private set; }

    public Transform targetTransform { get; private set; }
    public FovInfo fov { get; private set; }

    public float seenTimeDiff { get; private set; }
    public float lastSeenTime { get; private set; }
    public float minimumTimeTreshold { get; set; }

    public bool isPlayerVisible { get; private set; }
    public bool canDelayInvestigation { get; private set; }
    public bool canInvestigateDisturbance { get; set; }
    public bool canAgentPatrol { get; private set; }
    
    public int aiGUID { get; private set; }
    public InvestigateReason currentInvestigateReason { get; private set; }

    public EnemyIndicator indicator { get; private set; }
    /********* PRIVATE *********/

    private float seenTimeMultiplier = 1.0f;
    private float lastSeenTimeDiff;
    private float trackingTimeDiff;

    private bool isTimeMultiplierRunning = false;
    private bool wasPlayerPreviouslySeen = false;
    private bool isStateChangeRequired = false;

    private void Start()
    {
        aiGUID = AIManager.instance.GetNextAssignableEntryId();
        AIManager.instance.RegisterAgent(this);
        
        targetTransform = FindObjectOfType<PlayerShootingController>()?.transform;
        Assert.IsNotNull(targetTransform);

        Assert.IsNotNull(indicatorPrefab);
        indicator = ObjectBuilder.CreateAndAddObjectToCanvas(indicatorPrefab)?.GetComponent<EnemyIndicator>();

        Assert.IsNotNull(indicator);
        Assert.IsNotNull(trackerTransform);

        indicator.SetTrackedTransform(trackerTransform);

        fsm             = new FSM<EnemyAI>(this);
        health          = GetComponent<Health>();
        navMeshAgent    = GetComponent<NavMeshAgent>();
        fov             = new FovInfo
        {
            maxAngle    = maxViewAngle,
            maxDistance = maxViewDistance,
            layerMask   = blockingLayerMask
        };

        health.OnDeath += OnDeath;

        spawnPosition  = transform.position;
        canAgentPatrol = canPatrol;

        if (canPatrol)
            fsm.ChangeState<EnemyStatePatrol>();
        else
            fsm.ChangeState<EnemyStateIdle>();

        currentInvestigateReason = InvestigateReason.None;
    }

    private void Update()
    {
        isPlayerVisible = Visibility.CanSeeObject(transform, targetTransform, fov, maxSearchRadius);

        UpdateSeenTimeDiff();
        UpdateStatesAndConditions();
        UpdateTrackingProgress();
    }

    private void UpdateStatesAndConditions()
    {
        CheckStatesCondition();

        if (isStateChangeRequired)
            ChangeStates();
    }

    private void CheckStatesCondition()
    {
        if (!isPlayerVisible)
        {
            if (wasPlayerPreviouslySeen)
            {
                lastSeenTime            = Time.time - Time.deltaTime;
                lastSeenTimeDiff        = seenTimeDiff;
                wasPlayerPreviouslySeen = false;
            }

            if (GetTimeSinceLastPlayerSeen() < secondsToRememberPlayer)
                lastKnownPlayerPosition = targetTransform.position;

            return;
        }

        if (!wasPlayerPreviouslySeen)
            wasPlayerPreviouslySeen = true;

        if (!isTimeMultiplierRunning)
            StartCoroutine(UpdateSeenTimeMultiplier());

        lastKnownPlayerPosition = targetTransform.position;

        if (GetTimeSinceLastPlayerSeen() < secondsToRememberPlayer && seenTimeDiff < lastSeenTimeDiff)
            seenTimeDiff = lastSeenTimeDiff;
    }

    private void UpdateSeenTimeDiff()
    {
        float oldSeenTimeDiff   = seenTimeDiff;
        seenTimeDiff            += isPlayerVisible ? Time.deltaTime * seenTimeMultiplier : -Time.deltaTime;
        seenTimeDiff            = Mathf.Max(minimumTimeTreshold, seenTimeDiff);
        isStateChangeRequired   = IsStateChangeRequired(oldSeenTimeDiff);
        seenTimeDiff            = Mathf.Clamp(seenTimeDiff, 0f, secondsToChase);
    }

    private void UpdateTrackingProgress()
    {
        float t;
        
        if (seenTimeDiff <= secondsToInvestigate)
            t = Mathf.InverseLerp(0f, secondsToInvestigate, seenTimeDiff);
        else
            t = 1f + Mathf.InverseLerp(secondsToInvestigate, secondsToChase, seenTimeDiff);
        
        // Update EnemyIndicator color
        indicator.SetState(t);
    }

    private bool IsStateChangeRequired(float oldSeenTimeDiff)
    {
        return (oldSeenTimeDiff < 1.0f && seenTimeDiff >= secondsToInvestigate) ||
            (oldSeenTimeDiff < secondsToChase && seenTimeDiff >= secondsToChase);
    }

    private IEnumerator UpdateSeenTimeMultiplier()
    {
        isTimeMultiplierRunning = true;

        yield return new WaitForSeconds(0.5f);

        while (isPlayerVisible)
        {
            Vector3 toOther = targetTransform.position - transform.position;
            toOther = toOther.ProjectOntoPlane(Vector3.up);

            // Check if the player is within 30% of its total cone
            bool canIncreaseMultiplier =
                Vector3.Angle(toOther, transform.forward) < fov.maxAngle * 0.5f * 0.3f;

            if (canIncreaseMultiplier)
            {
                float distance              = Mathf.Min(toOther.magnitude, fov.maxDistance);
                float bonusMultiplier       = 0.08f;
                float maxBonusMultiplier    = 0.4f;
                seenTimeMultiplier          += Mathf.Min(maxBonusMultiplier, maxBonusMultiplier - (bonusMultiplier * distance / 100.0f));
                Mathf.Clamp(seenTimeMultiplier, 1f, 2.0f);
            }
            else seenTimeMultiplier = 1.0f;

            yield return null;
        }

        isTimeMultiplierRunning = false;
        seenTimeMultiplier      = 1.0f;
    }

    public void On(Distraction shot)
    {
        if (health.isDead)
            return;

        float distanceToTheShot = (shot.position - transform.position).sqrMagnitude;

        if (distanceToTheShot > disturbanceHearingRadius * disturbanceHearingRadius)
            return;

        lastInvestigatePosition = shot.position;

        if (canInvestigateDisturbance)
        {
            canDelayInvestigation     = true;
            isStateChangeRequired     = false;
            currentInvestigateReason  = InvestigateReason.AllyDeath;
            
            fsm.ChangeState<EnemyStateInvestigate>();
        }
    }

    public bool CloseToLastKnownPlayerLocation()
    {
        return (transform.position - lastKnownPlayerPosition).sqrMagnitude <
               stoppingDistanceBeforeLastPlayerPosition * stoppingDistanceBeforeLastPlayerPosition;
    }

    public float GetTimeSinceLastPlayerSeen()
    {
        return Time.time - lastSeenTime;
    }

    private void ChangeStates()
    {
        isStateChangeRequired = false;

        if (seenTimeDiff >= secondsToChase)
            fsm.ChangeState<EnemyStateChasePlayer>();
        else if (seenTimeDiff >= secondsToInvestigate)
        {
            canDelayInvestigation   = false;

            if (canInvestigateDisturbance)
            {
                currentInvestigateReason = InvestigateReason.PlayerSeen;
                lastInvestigatePosition = targetTransform.position;
            }

            fsm.ChangeState<EnemyStateInvestigate>();
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        if (indicator != null)
            Destroy(indicator.gameObject);

        indicator = null;
    }

    private void OnDeath(Health sender)
    {
        AIManager.instance.UnregisterAgent(this);
        
        new Distraction(transform.position)
            .PostEvent();
    }
}
