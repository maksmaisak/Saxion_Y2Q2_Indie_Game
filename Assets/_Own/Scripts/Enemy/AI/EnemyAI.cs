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
        [Header("AI Search Settings")] 
        [SerializeField] Transform visionOrigin;
        [SerializeField] LayerMask blockingLayerMask = Physics.DefaultRaycastLayers;
        [SerializeField] float maxViewAngle = 60.0f;
        [SerializeField] float maxViewDistance = 20.0f;
        [SerializeField] float defaultDisturbanceHearingRadius = 15f;
        [SerializeField] float maxSearchRadius = 5.0f;
        [SerializeField] float roaringRadius = 7.0f;
        [SerializeField] float maxFootstepsHearingRadius = 9.0f;
        [SerializeField] float footstepsHearingRadiusWhileCovered = 4.0f; 
        [Space]
        [Header("AI Settings")]
        [SerializeField] float chaseAwarenessLevel = 2.0f;
        [SerializeField] float investigateAwarenessLevel = 1.0f;
        [SerializeField] float secondsToRememberPlayer = 2.0f;
        [SerializeField] float secondsBetweenInvestigations = 3.0f;
        [Range(0.1f, 10f)]
        [SerializeField] float maxDistractionPointOffsetRange = 10f;
        [SerializeField] bool canPatrol = false;
        [Space]
        [Header("AI Movement")]
        public float chaseSpeed = 2.3f;
        public float patrolSpeed = 0.6f;
        public float wanderSpeed = 1.4f;
        public float investigateSpeed = 1.2f;
        public float goBackSpeed = 1f;
        [Header("AI Assignable")]
        [SerializeField] GameObject indicatorPrefab;
        [SerializeField] Transform trackerTransform;
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
        public EnemyIndicator indicator { get; private set; }
        public Rigidbody playerRigidbody { get; private set; }
        /********* PRIVATE *********/

        private GameObject attachedObjectHit;
        
        private float awarenessLevelMultiplier = 1.0f;
        private float hearingFootstepsDiff;
        private float lastAwarenesLevel;
        private float trackingTimeDiff;
    
        private bool isAwarenessMultiplierRunning = false;
        private bool wasPlayerPreviouslySeen = false;
        private bool isStateChangeRequired = false;
        private bool alreadyCallAssistance = false;
        private bool canInvestigateDisturbance = true;
        private bool isAlreadyBeingHit = false;

        private Animator playerAnimator;
        
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
    
        private void Start()
        {
            aiGUID = AIManager.instance.GetNextAssignableEntryId();
            AIManager.instance.RegisterAgent(this);
    
            if (!visionOrigin) visionOrigin = transform;
    
            visionOriginTransform = visionOrigin;
    
            Assert.IsTrue(PlayerVisibilityCenter.exists);
            targetTransform = PlayerVisibilityCenter.instance.transform;
    
            Assert.IsNotNull(indicatorPrefab);
            indicator = CanvasObjectBuilder.CreateAndAddObjectToCanvas(indicatorPrefab)?.GetComponent<EnemyIndicator>();
    
            Assert.IsNotNull(indicator);
            Assert.IsNotNull(trackerTransform);
    
            indicator.SetTrackedTransform(trackerTransform);
            indicator.SetTrackedRenderer(GetComponentInChildren<Renderer>());
    
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
    
        private void Update()
        {
            isPlayerVisible = Visibility.CanSeeObject(visionOrigin, targetTransform, fov, maxSearchRadius);
    
            if (CanHearPlayer())
                hearingFootstepsDiff += Time.deltaTime;
            else
                hearingFootstepsDiff = 0;
            
            UpdateAwarenessLevel();
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
            indicator.SetState(t);
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
            float hearingRadiusSqr  = maxFootstepsHearingRadius * maxFootstepsHearingRadius;
            
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

            // TODO would be better with attachedObjectHit == head, with head being assigned in the inspector.
            new OnEnemyDied(attachedObjectHit && attachedObjectHit.CompareTag("EnemyHead")).PostEvent();
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
    
        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            if (indicator != null)
                Destroy(indicator.gameObject);
    
            indicator = null;
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
            
            indicator.ShowAlertHeardSomething();
    
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

        public void SetAttachedObjectHit(GameObject newGameObject)
        {
            if(isAlreadyBeingHit)
                return;

            isAlreadyBeingHit = true;

            attachedObjectHit = newGameObject;

            this.Delay(0.05f, () => isAlreadyBeingHit = false);
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

            float distanceSqr = (investigation.distractionPoint - transform.position).sqrMagnitude;
            float hearingRadius = investigation.enemyHearingRadius ?? defaultDisturbanceHearingRadius;
            return distanceSqr <= hearingRadius * hearingRadius;
        }

        public void SetInvestigateNewDisturbance(bool enable)
        {
            canInvestigateDisturbance = enable;
        }

        public bool IsPlayerCrouching() => playerAnimator.GetBool("Crouch");
    }