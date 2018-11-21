using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using  Cinemachine.Utility;
using Unity.Collections;
using UnityEngine.Assertions;

public enum AIState
{
    Idle = 0,
    Distracted,
    InvestigatePlayer,
    InvestigateNoise,
    ChasePlayer,
    LookAround,
    Wander,
    GoingBack,
    None,
}

[DisallowMultipleComponent]
[RequireComponent(typeof(NavMeshAgent), typeof(Health))]
public class EnemyAI : MonoBehaviour, IEventReceiver<Disturbance>
{
    [Header("AI Search Settings")]
    [SerializeField] LayerMask blockingLayerMask = Physics.DefaultRaycastLayers;
    [SerializeField] float maxViewAngle = 60.0f;
    [SerializeField] float maxViewDistance = 20.0f;
    [SerializeField] float disturbanceHearingRadius = 10f;
    [Space]
    [Header("AI Movement")]
    [SerializeField] float stoppingDistanceBeforeLastPlayerPosition = 2f;

    /********* PUBLIC *********/
    public Health health { get; private set; }

    public FSM<EnemyAI> fsm { get; private set; }
    public NavMeshAgent navMeshAgent { get; private set; }

    public Vector3? lastHeardDisturbancePositions { get; set; }
    public Vector3 lastKnownPlayerPosition { get; private set; }
    public Vector3 spawnPosition { get; private set; }

    public Transform targetTransform { get; private set; }
    public bool isPlayerVisible { get; private set; }
    public FovInfo fov { get; private set; }

    public AIState currentAIState { get; private set; }

    public float seenTimeDiff { get; private set; }
    public float lastSeenTime { get; private set; }
    /********* PRIVATE *********/
    private AIState previousAIState;

    private float seenTimeMultiplier = 1.0f;
    private float lastSeenTimeDiff;

    private bool isTimeMultiplierRunning = false;
    private bool wasPlayerPreviouslySeen = false;
    
    void Start()
    {
        targetTransform = FindObjectOfType<PlayerShootingController>()?.transform;
        Assert.IsNotNull(targetTransform);

        fsm = new FSM<EnemyAI>(this);
        health = GetComponent<Health>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        fov = new FovInfo
        {
            maxAngle = maxViewAngle,
            maxDistance = maxViewDistance,
            layerMask = blockingLayerMask
        };

        health.OnDeath += sender => new Disturbance(gameObject).PostEvent();

        previousAIState = AIState.None;
        currentAIState = AIState.None;

        fsm.ChangeState<EnemyStateIdle>();
        spawnPosition = transform.position;
    }

    public void Update()
    {
        LookForPlayer();
    }

    public void LookForPlayer()
    {
        isPlayerVisible = Visibility.CanSeeObject(transform, targetTransform, fov);

        if (isPlayerVisible)
        {
            if (!wasPlayerPreviouslySeen)
                wasPlayerPreviouslySeen = true;
            
            if (!isTimeMultiplierRunning)
                StartCoroutine(UpdateSeenTimeMultiplier());

            seenTimeDiff += Time.deltaTime * seenTimeMultiplier;
            lastKnownPlayerPosition = targetTransform.position;

            if (Time.time - lastSeenTime < 0.5f)
                seenTimeDiff = lastSeenTimeDiff;

            if ((currentAIState == AIState.Idle || currentAIState == AIState.GoingBack))
                fsm.ChangeState<EnemyStateDistracted>();

            if (currentAIState == AIState.Wander && previousAIState == AIState.ChasePlayer && seenTimeDiff >= 1.5f)
                fsm.ChangeState<EnemyStateChasePlayer>();
        }
        else
        {
            if (wasPlayerPreviouslySeen)
            {
                lastSeenTime             = Time.time;
                lastSeenTimeDiff         = seenTimeDiff;
                seenTimeDiff             = 0;
                wasPlayerPreviouslySeen  = false;
            }
        }
    }

    public void On(Disturbance shot)
    {
        if (shot.gameObject == gameObject)
            return;

        if ((shot.gameObject.transform.position - transform.position).sqrMagnitude > disturbanceHearingRadius * disturbanceHearingRadius)
            return;

        lastHeardDisturbancePositions = shot.gameObject.transform.position;

        if (currentAIState == AIState.Idle || currentAIState == AIState.GoingBack || currentAIState == AIState.Wander || currentAIState == AIState.LookAround)
            fsm.ChangeState<EnemyStateInvestigateNoise>();
    }

    private IEnumerator UpdateSeenTimeMultiplier()
    {
        isTimeMultiplierRunning = true;

        yield return new WaitForSeconds(0.5f);

        while (enabled)
        {
            Vector3 toOther = targetTransform.position - transform.position;
            toOther = toOther.ProjectOntoPlane(Vector3.up);

            bool canIncreaseMultiplier =
                Vector3.Angle(toOther, transform.forward) < fov.maxAngle * 0.3f;

            if (canIncreaseMultiplier)
            {
                float distance              = Mathf.Min(toOther.magnitude, fov.maxDistance);
                float bonusMultiplier       = 0.08f;
                float maxBonusMultiplier    = 0.8f;
                seenTimeMultiplier          += Mathf.Min(maxBonusMultiplier, maxBonusMultiplier - (bonusMultiplier * distance / 100.0f));
            }
            else if (!isPlayerVisible)
            {
                seenTimeMultiplier = 1.0f;
                break;
            }
            else seenTimeMultiplier = 1.0f;

            yield return null;
        }

        isTimeMultiplierRunning = false;
    }

    public bool CloseToLastKnownPlayerLocation()
    {
        return (transform.position - lastKnownPlayerPosition).sqrMagnitude <
               stoppingDistanceBeforeLastPlayerPosition * stoppingDistanceBeforeLastPlayerPosition;
    }

    public void SetAIState(AIState newState)
    {
        previousAIState = currentAIState;
        currentAIState = newState;
    }
}
