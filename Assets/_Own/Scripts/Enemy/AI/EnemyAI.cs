using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using Cinemachine.Utility;
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
    [SerializeField] Transform visionOrigin;
    [SerializeField] LayerMask blockingLayerMask = Physics.DefaultRaycastLayers;
    [SerializeField] float maxViewAngle = 60.0f;
    [SerializeField] float maxViewDistance = 20.0f;
    [SerializeField] float disturbanceHearingRadius = 10f;
    [SerializeField] float maxSearchRadius = 5.0f;
    [Space]
    [Header("AI Settings")]
    [SerializeField] float assistanceRadius = 7.0f;
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

    public Transform visionOriginTransform { get; private set; }
    public Transform targetTransform { get; private set; }
    public FovInfo fov { get; private set; }

    public float awarenessLevel { get; private set; }
    public float lastSeenTime { get; private set; }
    public float minimumTimeThreshold { get; set; }

    public bool isPlayerVisible { get; private set; }
    public bool canDelayInvestigation { get; private set; }
    public bool canInvestigateDisturbance { get; set; }
    public bool canAgentPatrol { get; private set; }
    
    public int aiGUID { get; private set; }
    public InvestigateReason currentInvestigateReason { get; private set; }

    public EnemyIndicator indicator { get; private set; }
    /********* PRIVATE *********/

    private float seenTimeMultiplier = 1.0f;
    private float lastAwarenesLevel;
    private float trackingTimeDiff;

    private bool isTimeMultiplierRunning = false;
    private bool wasPlayerPreviouslySeen = false;
    private bool isStateChangeRequired = false;
    private bool alreadyCallAssistance = false;

    private void Start()
    {
        aiGUID = AIManager.instance.GetNextAssignableEntryId();
        AIManager.instance.RegisterAgent(this);

        if (!visionOrigin) visionOrigin = transform;

        visionOriginTransform = visionOrigin;

        Assert.IsTrue(PlayerVisibilityCenter.exists);
        targetTransform = PlayerVisibilityCenter.instance.transform;

        Assert.IsNotNull(indicatorPrefab);
        indicator = ObjectBuilder.CreateAndAddObjectToCanvas(indicatorPrefab)?.GetComponent<EnemyIndicator>();

        Assert.IsNotNull(indicator);
        Assert.IsNotNull(trackerTransform);

        indicator.SetTrackedTransform(trackerTransform);
        indicator.SetTrackedRenderer(GetComponentInChildren<Renderer>());

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
        isPlayerVisible = Visibility.CanSeeObject(visionOrigin, targetTransform, fov, maxSearchRadius);

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
                lastSeenTime             = Time.time - Time.deltaTime;
                lastAwarenesLevel        = awarenessLevel;
                wasPlayerPreviouslySeen  = false;
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

        if (GetTimeSinceLastPlayerSeen() < secondsToRememberPlayer && awarenessLevel < lastAwarenesLevel)
            awarenessLevel = lastAwarenesLevel;
    }

    private void UpdateSeenTimeDiff()
    {
        float oldAwarenessLevelDiff  = awarenessLevel;
        awarenessLevel              += isPlayerVisible ? Time.deltaTime * seenTimeMultiplier : -Time.deltaTime;
        awarenessLevel               = Mathf.Max(minimumTimeThreshold, awarenessLevel);
        isStateChangeRequired        = IsStateChangeRequired(oldAwarenessLevelDiff);
        awarenessLevel               = Mathf.Clamp(awarenessLevel, 0f, secondsToChase);
    }

    private void UpdateTrackingProgress()
    {
        float t;

        if (awarenessLevel <= secondsToInvestigate)
            t = Mathf.InverseLerp(0f, secondsToInvestigate, awarenessLevel);
        else
            t = 1f + Mathf.InverseLerp(secondsToInvestigate, secondsToChase, awarenessLevel);

        // Update EnemyIndicator color
        indicator.SetState(t);
    }

    private void CallAssistance()
    {
        if(!alreadyCallAssistance)
        {
            List<EnemyAI> assistList = AIManager.instance.GetAllAssistAgentsInRange(this, assistanceRadius);

            foreach (EnemyAI agent in assistList)
                agent.StartAttackPlayer();
        }
    }

    public void SetNoCallAssistance(bool enable)
    {
        alreadyCallAssistance = enable;
    }

    public bool CanAssist()
    {
        if (!canInvestigateDisturbance)
            return false;

        return true;
    }

    public void StartAttackPlayer()
    {
        awarenessLevel            = secondsToChase + 0.2f; // Set this manually to prevent changeing states multiple times
        lastKnownPlayerPosition   = targetTransform.position;

        fsm.ChangeState<EnemyStateChasePlayer>();
    }

    private bool IsStateChangeRequired(float oldSeenTimeDiff)
    {
        return (oldSeenTimeDiff < 1.0f && awarenessLevel >= secondsToInvestigate) ||
            (oldSeenTimeDiff < secondsToChase && awarenessLevel >= secondsToChase);
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

    public bool IsCloseToLastKnownPlayerLocation()
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

        if (awarenessLevel >= secondsToChase)
        {
            CallAssistance();
            fsm.ChangeState<EnemyStateChasePlayer>();
        }
        else if (awarenessLevel >= secondsToInvestigate)
        {
            canDelayInvestigation = false;

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
