using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using Cinemachine.Utility;
using UnityEngine.Assertions;
using UnityEditor;
using UnityEngine.Events;
using UnityEngine.Serialization;

[DisallowMultipleComponent]
[RequireComponent(typeof(NavMeshAgent), typeof(Health))]
public class EnemyAI : MyBehaviour, ISerializationCallbackReceiver
{        
    [Header("Vision")] 
    [SerializeField] Transform visionOrigin;
    [SerializeField] LayerMask blockingLayerMask = Physics.DefaultRaycastLayers;
    [SerializeField] [Range(0f, 360f)] float maxViewAngle = 60.0f;
    [SerializeField] float maxViewDistance = 20.0f;
    [SerializeField] float fullViewRadius = 5.0f;
    [Header("Hearing")]
    [SerializeField] float footstepsHearingRadius = 9.0f;
    [SerializeField] float footstepsHearingRadiusWhileCovered = 4.0f;
    [Header("Behavior")]
    [SerializeField] float chaseAwarenessLevel = 2.0f;
    [SerializeField] float investigateAwarenessLevel = 1.0f;
    [SerializeField] float secondsToRememberPlayer = 2.0f;
    [SerializeField] float secondsBetweenInvestigations = 3.0f;
    [SerializeField] [Range(0.1f, 10f)] float maxDistractionPointOffsetRange = 10f;
    [SerializeField] float roaringRadius = 7.0f;
    [SerializeField] bool canPatrol = false;
    [Space]
    [Header("Movement")]
    public float chaseSpeed = 2.3f;
    public float patrolSpeed = 0.6f;
    public float wanderSpeed = 1.4f;
    public float investigateSpeed = 1.2f;
    public float goBackSpeed = 1f;

    [Header("Assignables")] 
    [FormerlySerializedAs("indicatorPrefab")] [SerializeField] GameObject awarenessLevelIndicatorPrefab;
    [FormerlySerializedAs("trackerTransform")] [SerializeField] Transform indicatorLocation;
    [SerializeField] ShootingController _shootingController;

    [SerializeField] UnityEvent OnCallForAssistance;

    /********* PUBLIC *********/
    public Health health { get; private set; }

    public FSM<EnemyAI> fsm { get; private set; }
    public NavMeshAgent navMeshAgent { get; private set; }
    public ShootingController shootingController => _shootingController;
    
    public Vector3 spawnPosition { get; private set; }
    public Vector3? lastInvestigatePosition { get; set; }
    public Vector3 lastKnownPlayerPosition { get; private set; }

    public Transform visionOriginTransform { get; private set; }
    public Transform targetTransform { get; private set; }
    public FovInfo fov { get; private set; }

    public float awarenessLevel { get; private set; }
    public float lastSeenTime { get; private set; }
    public float minimumAwarenessLevelThreshold { get; set; }
    
    public bool isPlayerVisible { get; private set; }
    public bool isInvestigating { get; set; }
    public bool canDelayInvestigation { get; private set; }
    public bool canAgentPatrol { get; private set; }
    public bool canWanderOnInvestigationFinish { get; private set; }
    
    public int aiGUID { get; private set; }

    public Investigation currentInvestigation { get; private set; }
    public EnemyIndicator awarenessLevelIndicator { get; private set; }
    public Rigidbody playerRigidbody { get; private set; }
    /********* PRIVATE *********/

    private  float awarenessLevelMultiplier = 1.0f;
    private float hearingFootstepsDiff;
    private float lastAwarenesLevel;
    private float trackingTimeDiff;

    private bool isAwarenessMultiplierRunning = false;
    private bool wasPlayerPreviouslySeen = false;
    private bool isStateChangeRequired = false;
    private bool alreadyCallAssistance = false;
    private bool canInvestigateDisturbance = true;

    private Animator playerAnimator;
    
    void OnValidate()
    {
        footstepsHearingRadiusWhileCovered = Mathf.Clamp(footstepsHearingRadiusWhileCovered, 0f, footstepsHearingRadius);
    }
    
    #if UNITY_EDITOR
    
    [CustomEditor(typeof(EnemyAI), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    public class EnemyAIEditor : Editor
    {
        private readonly Color visionSurfaces = new Color(1f, 0f, 0f, 0.02f); 
        private readonly Color visionHandles  = new Color(1f, 0f, 0f, 1f   ); 
        
        private readonly Color hearingSurfaces = new Color(0f, 0f, 1f, 0.02f); 
        private readonly Color hearingHandles  = new Color(0f, 0f, 1f, 1f   );
    
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            serializedObject.ApplyModifiedProperties();
        }
    
        void OnSceneGUI()
        {
            var ai = (EnemyAI)target;
            float maxViewDistance = ai.maxViewDistance;
            float fullViewRadius  = ai.fullViewRadius;
            float footstepsHearingRadius = ai.footstepsHearingRadius;
            float footstepsHearingRadiusWhileCovered = ai.footstepsHearingRadiusWhileCovered;

            Transform transform = ai.visionOrigin ? ai.visionOrigin : ai.transform;
            Vector3 position = transform.position;
            Vector3 up = transform.up;
            Vector3 forward = transform.forward;
            Quaternion rotation = transform.rotation;
                        
            EditorGUI.BeginChangeCheck();

            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
            {
                Handles.color = visionSurfaces;
                DrawViewArc(
                    position,
                    up,
                    forward,
                    ai.maxViewAngle,
                    ai.maxViewDistance
                );
                Handles.color = visionHandles;
                maxViewDistance = Handles.RadiusHandle(rotation, position, maxViewDistance);
            }

            {
                Handles.color = visionSurfaces;
                DrawViewArc(
                    position,
                    up,
                    -forward,
                    360f - ai.maxViewAngle,
                    ai.fullViewRadius
                );
                Handles.color = visionHandles;
                fullViewRadius = Handles.RadiusHandle(rotation, position, fullViewRadius);
            }

            {
                Handles.color = hearingSurfaces;
                Handles.DrawSolidDisc(position, up, footstepsHearingRadius);

                Handles.color = hearingHandles;
                footstepsHearingRadius = Handles.RadiusHandle(rotation, position, footstepsHearingRadius);
            }

            {
                Handles.color = hearingSurfaces;
                Handles.DrawSolidDisc(position, up, ai.footstepsHearingRadiusWhileCovered);
                
                Handles.color = hearingHandles;
                footstepsHearingRadiusWhileCovered = 
                    Handles.RadiusHandle(rotation, position, footstepsHearingRadiusWhileCovered);

                footstepsHearingRadiusWhileCovered =
                    Mathf.Clamp(footstepsHearingRadiusWhileCovered, 0f, footstepsHearingRadius);
            }

            if (!EditorGUI.EndChangeCheck()) return;
            
            Undo.RecordObject(ai, "Change AI settings");
            ai.maxViewDistance = maxViewDistance;
            ai.fullViewRadius = fullViewRadius;
            ai.footstepsHearingRadius = footstepsHearingRadius;
            ai.footstepsHearingRadiusWhileCovered = footstepsHearingRadiusWhileCovered;
        }
        
        void DrawViewArc(Vector3 position, Vector3 up, Vector3 forward, float angle, float radius)
        {
            Handles.DrawSolidArc(
                position,
                up,
                Quaternion.AngleAxis(-angle / 2, up) * forward,
                angle,
                radius
            );
        }
    }
    
    #endif
    
    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
    #if UNITY_EDITOR
        // Prevent saving if this is a prefab
        PrefabType prefabType = PrefabUtility.GetPrefabType(this);
        if (prefabType == PrefabType.Prefab || prefabType == PrefabType.ModelPrefab )
            canPatrol = false;
    #endif
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize() {}

    void Start()
    {
        aiGUID = AIManager.instance.GetNextAssignableEntryId();
        AIManager.instance.RegisterAgent(this);

        if (!visionOrigin) visionOrigin = transform;

        visionOriginTransform = visionOrigin;

        if (PlayerVisibilityCenter.exists)
        {
            targetTransform = PlayerVisibilityCenter.instance.transform;
        }
        else
        {
            Debug.LogWarning("No PlayerVisibilityCenter found. Defaulting to transform of `PlayerCharacter`");
            targetTransform = GameObject.Find("PlayerCharacter")?.transform;
        }
        Assert.IsNotNull(targetTransform);

        Assert.IsNotNull(awarenessLevelIndicatorPrefab);
        awarenessLevelIndicator = CanvasObjectBuilder.CreateAndAddObjectToCanvas(awarenessLevelIndicatorPrefab)?.GetComponent<EnemyIndicator>();

        Assert.IsNotNull(awarenessLevelIndicator);
        Assert.IsNotNull(indicatorLocation);

        awarenessLevelIndicator.SetTrackedTransform(indicatorLocation);
        awarenessLevelIndicator.SetTrackedRenderer(GetComponentInChildren<Renderer>());

        playerAnimator = targetTransform.GetComponentInParent<Animator>();
        Assert.IsNotNull(playerAnimator);

        playerRigidbody = targetTransform.GetComponentInParent<Rigidbody>();
        Assert.IsNotNull(playerRigidbody);

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
        shootingController.Initialize(gameObject);
        
        spawnPosition                  = transform.position;
        canAgentPatrol                 = canPatrol;
        canWanderOnInvestigationFinish = false;

        if (canPatrol)
            fsm.ChangeState<EnemyStatePatrol>();
        else
            fsm.ChangeState<EnemyStateIdle>();
    }

    void Update()
    {
        isPlayerVisible = Visibility.CanSeeObject(visionOrigin, targetTransform, fov, fullViewRadius);

        if (CanHearPlayer())
            hearingFootstepsDiff += Time.deltaTime;
        else
            hearingFootstepsDiff = 0;
        
        UpdateAwarenessLevel();
        UpdateStatesAndConditions();
        UpdateTrackingProgress();
    }

    void OnDisable() => fsm.Disable();

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        if (awarenessLevelIndicator != null)
            Destroy(awarenessLevelIndicator.gameObject);

        awarenessLevelIndicator = null;
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

        if (!isAwarenessMultiplierRunning)
            StartCoroutine(UpdateSeenTimeMultiplier());

        lastKnownPlayerPosition = targetTransform.position;

        if (GetTimeSinceLastPlayerSeen() < secondsToRememberPlayer && awarenessLevel < lastAwarenesLevel)
            awarenessLevel = lastAwarenesLevel;
    }

    private void UpdateAwarenessLevel()
    {
        float oldAwarenessLevelDiff  = awarenessLevel;
        awarenessLevel              += hearingFootstepsDiff;
        awarenessLevel              += isPlayerVisible ? Time.deltaTime * awarenessLevelMultiplier : 
                                       hearingFootstepsDiff > 0 ? 0 : -Time.deltaTime;
        awarenessLevel               = Mathf.Max(minimumAwarenessLevelThreshold, awarenessLevel);
        isStateChangeRequired        = IsStateChangeRequired(oldAwarenessLevelDiff);
        awarenessLevel               = Mathf.Clamp(awarenessLevel, 0f, chaseAwarenessLevel);
    }

    private void UpdateTrackingProgress()
    {
        float t;

        if (awarenessLevel <= investigateAwarenessLevel)
            t = Mathf.InverseLerp(0f, investigateAwarenessLevel, awarenessLevel);
        else
            t = 1f + Mathf.InverseLerp(investigateAwarenessLevel, chaseAwarenessLevel, awarenessLevel);

        // Update EnemyIndicator color
        awarenessLevelIndicator?.SetState(t);
    }

    private void CallAssistance()
    {
        if(!alreadyCallAssistance)
        {
            List<EnemyAI> assistList = AIManager.instance.GetAllAssistAgentsInRange(this, roaringRadius);

            foreach (EnemyAI agent in assistList)
                agent.StartAttackPlayer();

            OnCallForAssistance.Invoke();
        }
    }

    private bool CanHearPlayer()
    {
        // No need to check for that since the agent is already chasing the player
        if (awarenessLevel >= chaseAwarenessLevel)
            return false;
        
        // The player should be moving
        if (playerRigidbody.velocity.magnitude < 1.0f)
            return false;
        
        // The player should not be crouching
        if (IsPlayerCrouching())
            return false;
        
        Vector3 delta           = (targetTransform.position - visionOrigin.transform.position);
        float distanceSqr       = delta.sqrMagnitude;
        float hearingRadiusSqr  = footstepsHearingRadius * footstepsHearingRadius;
        
        // The player should be within max hearing radius
        if (distanceSqr > hearingRadiusSqr)
            return false;
        
        Ray ray = new Ray(visionOriginTransform.position, delta.normalized);
        
        // If the player is on the other side of the wall
        // the hearing radius is reduced.
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, delta.magnitude + 1.0f, blockingLayerMask))
            if (hit.distance * hit.distance < distanceSqr)
                hearingRadiusSqr = footstepsHearingRadiusWhileCovered * footstepsHearingRadiusWhileCovered;
                
        // Check the distance again, the player might be covered, therefore the hearing radius is reduced.
        if (distanceSqr > hearingRadiusSqr)
            return false;
        
        return true;
    }
    
    private bool IsStateChangeRequired(float oldSeenTimeDiff)
    {
        return (oldSeenTimeDiff < 1.0f && awarenessLevel >= investigateAwarenessLevel) ||
            (oldSeenTimeDiff < chaseAwarenessLevel && awarenessLevel >= chaseAwarenessLevel);
    }

    private IEnumerator UpdateSeenTimeMultiplier()
    {
        isAwarenessMultiplierRunning = true;

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
                awarenessLevelMultiplier    += Mathf.Min(maxBonusMultiplier, maxBonusMultiplier - (bonusMultiplier * distance / 100.0f));
                Mathf.Clamp(awarenessLevelMultiplier, 1f, 2.0f);
            }
            else awarenessLevelMultiplier = 1.0f;

            yield return null;
        }

        isAwarenessMultiplierRunning         = false;
        awarenessLevelMultiplier        = 1.0f;
    }

    private void OnDeath(Health sender)
    {
        AIManager.instance.UnregisterAgent(this);

        if (awarenessLevelIndicator != null)
            Destroy(awarenessLevelIndicator.gameObject);
        awarenessLevelIndicator = null;

        AIManager.instance.RegisterDeadAgent(this);
        
        GetComponent<EnemyLootable>().ShowIndicator();
    }

    private void ChangeStates()
    {
        isStateChangeRequired = false;

        if (awarenessLevel >= chaseAwarenessLevel)
        {
            CallAssistance();
            fsm.ChangeState<EnemyStateChasePlayer>();
        }
        else if (awarenessLevel >= investigateAwarenessLevel)
        {
            canDelayInvestigation = false;

            if (canInvestigateDisturbance)
                lastInvestigatePosition = targetTransform.position;

            fsm.ChangeState<EnemyStateInvestigate>();
        }
    }

    public void SetWanderAfterInvestigation(bool enable)
    {
        // If the agent is already allowed to wander then don't do anything
        if (canWanderOnInvestigationFinish)
            return;
        
        canWanderOnInvestigationFinish = enable;
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

    public void Investigate(Investigation investigation)
    {
        canDelayInvestigation   = true;
        isStateChangeRequired   = false;
        lastInvestigatePosition = CalculateDistractionPoint(investigation);
        currentInvestigation    = investigation;
        currentInvestigation.OnInvestigationFinish += OnInvestigationFinish;
        
        // If already is investigating update navmeshagent
        if (isInvestigating)
            navMeshAgent.SetDestination(lastInvestigatePosition.Value);
        
        awarenessLevelIndicator.ShowAlertHeardSomething();

        fsm.ChangeState<EnemyStateInvestigate>();
    }

    private void OnInvestigationFinish(Investigation investigation)
    {
        if (currentInvestigation == investigation)
            currentInvestigation = null;
    }
    
    public void StartAttackPlayer()
    {
        isStateChangeRequired   = true;
        awarenessLevel          = chaseAwarenessLevel + 0.2f; // Set this manually to prevent changing states multiple times
        lastKnownPlayerPosition = targetTransform.position;
    }
    
    public float GetTimeSinceLastPlayerSeen()
    {
        return Time.time - lastSeenTime;
    }

    public Vector3 CalculateDistractionPoint(Investigation investigation)
    {
        // Increase or decrease the offset to the actual distractionPoint based on distractionPriority
        float distance         = Vector3.Distance(transform.position, investigation.distractionPoint);
        float radiusMultiplier = 4f;
        float offsetRadius     = Mathf.Max(0.1f, maxDistractionPointOffsetRange - (investigation.priority * distance * radiusMultiplier / 100f));
        Vector2 randomPoint    = Random.insideUnitCircle.normalized * offsetRadius;
        Vector3 delta          = new Vector3(randomPoint.x, 0.0f, randomPoint.y);
        
        return investigation.distractionPoint + delta;
    }

    public bool CanStartNewInvestigation(Investigation investigation)
    {
        if (health.isDead)
            return false;

        if (!canInvestigateDisturbance)
            return false;

        if (currentInvestigation != null && (currentInvestigation.priority > investigation.priority))
            if (investigation.startTime - currentInvestigation.startTime < secondsBetweenInvestigations)
                return false;

        float distanceSqr   = (investigation.distractionPoint - transform.position).sqrMagnitude;
        float hearingRadius = investigation.enemyHearingRadius;
        return distanceSqr <= hearingRadius * hearingRadius;
    }

    public void SetInvestigateNewDisturbance(bool enable)
    {
        canInvestigateDisturbance = enable;
    }

    public bool IsPlayerCrouching() => playerAnimator.GetBool("Crouch");
}