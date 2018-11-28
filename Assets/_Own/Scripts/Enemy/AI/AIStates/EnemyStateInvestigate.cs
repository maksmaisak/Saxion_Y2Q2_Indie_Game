    using System.Collections;
    using UnityEngine;
    using UnityEngine.AI;
    using UnityEngine.Assertions;
    using DG.Tweening;
    
    public class EnemyStateInvestigate : FSMState<EnemyAI>
    {
        [SerializeField] float minDelay = 0.1f;
        [SerializeField] float maxDelay = 1f;
        [SerializeField] float rotationDuration = 4f;
        [SerializeField] float lookAroundRotationAmount = 180.0f;
        [SerializeField] float minimumInvestigationTimeTreshold = 1.0f;
        [SerializeField] float investigationDuration = 10.0f;
    
        private Tween currentTween;
        private float investigationTimeDiff;
    
        void OnEnable()
        {
            investigationTimeDiff                   = investigationDuration;
            agent.isInvestigating                   = true;
            agent.navMeshAgent.speed                = agent.investigateSpeed;
            agent.minimumAwarenessLevelThreshold    = minimumInvestigationTimeTreshold;
    
            Assert.IsTrue(agent.lastInvestigatePosition.HasValue);
    
            agent.navMeshAgent.SetDestination(agent.lastInvestigatePosition.Value);
            
            StartCoroutine(Work());
        }
    
        void OnDisable()
        {
            currentTween?.Kill();
            
            currentTween          = null;
            agent.isInvestigating = false;
            
            StopAllCoroutines();
        }
    
        IEnumerator Work()
        {
            if (agent.canDelayInvestigation)
                yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));
    
            while (enabled)
            {    
                investigationTimeDiff -= Time.deltaTime;
    
                if (agent.isPlayerVisible)
                    investigationTimeDiff = investigationDuration;
    
                if (agent.navMeshAgent.remainingDistance <=
                    agent.navMeshAgent.stoppingDistance)
                {
                    if (currentTween == null)
                        StartCoroutine(StartRotation());
    
                    if (investigationTimeDiff <= 0f)
                        if (agent.canWanderOnInvestigationFinish) {
                            agent.fsm.ChangeState<EnemyStateWander>();
                        }
                        else
                            agent.fsm.ChangeState<EnemyStateGoBack>();
                }
    
    
                yield return null;
            }
        }
    
        IEnumerator StartRotation()
        {
            currentTween = transform.DORotate(Vector3.up * lookAroundRotationAmount, rotationDuration, RotateMode.WorldAxisAdd);
            yield return currentTween.WaitForCompletion();
    
            currentTween = transform.DORotate(Vector3.up * -lookAroundRotationAmount, rotationDuration, RotateMode.WorldAxisAdd);
            yield return currentTween.WaitForCompletion();
    
            currentTween = null;
        }
    }