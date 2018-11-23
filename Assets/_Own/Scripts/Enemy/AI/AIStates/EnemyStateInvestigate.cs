using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;
using DG.Tweening;

public class EnemyStateInvestigate : FSMState<EnemyAI>
{
    [SerializeField] float minDelay = 0.1f;
    [SerializeField] float maxDelay = 1f;
    [SerializeField] float stoppingDistance = 5f;
    [SerializeField] float rotationDuration = 4f;
    [SerializeField] float lookAroundRotationAmount = 180.0f;
    [SerializeField] float minimumInvestigationTimeTreshold = 1.0f;
    [SerializeField] float investigationDuration = 10.0f;

    private Tween currentTween;
    private float investigationTimeDiff;

    void OnEnable()
    {
        AIManager.instance.AssignInvestigator(agent);
        
        investigationTimeDiff       = investigationDuration;
        agent.navMeshAgent.speed    = agent.investigateSpeed;
        agent.minimumTimeTreshold   = minimumInvestigationTimeTreshold;
        
        StartCoroutine(Work());
    }

    void OnDisable()
    {
        if(Application.isPlaying)
            AIManager.instance.UnassignInvestigator(agent);
        
        currentTween?.Kill();
        currentTween = null;
        
        StopAllCoroutines();
    }

    IEnumerator Work()
    {
        if (agent.canDelayInvestigation)
            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

        while (enabled)
        {
            Assert.IsTrue(agent.lastInvestigatePosition.HasValue);

            agent.navMeshAgent.SetDestination(agent.lastInvestigatePosition.Value);

            investigationTimeDiff -= Time.deltaTime;

            if (agent.isPlayerVisible)
                investigationTimeDiff = investigationDuration;

            if (investigationTimeDiff <= 0f)
                if (AIManager.instance.CanWander(agent))
                {
                    agent.fsm.ChangeState<EnemyStateWander>();
                }
                else
                    agent.fsm.ChangeState<EnemyStateGoBack>();

            if (currentTween == null && !agent.navMeshAgent.pathPending && agent.navMeshAgent.remainingDistance <
                Mathf.Max(stoppingDistance, agent.navMeshAgent.stoppingDistance))
                StartCoroutine(StartRotation());

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