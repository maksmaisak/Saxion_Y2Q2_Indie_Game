using System.Collections;
using UnityEngine;
using DG.Tweening;

public class EnemyStateLookAround : FSMState<EnemyAI>
{
    [SerializeField] float duration = 4f;

    private Tween currentTween;
        
    void OnEnable() => StartCoroutine(Work());
    void OnDisable()
    {
        currentTween?.Kill();
        StopAllCoroutines();
    }

    IEnumerator Work()
    {
        currentTween = transform.DOPunchRotation(Vector3.up * 135f, duration, vibrato: 1);
        yield return currentTween.WaitForCompletion();
        
        currentTween = transform.DOPunchRotation(Vector3.up * -135f, duration, vibrato: 1);
        yield return currentTween.WaitForCompletion();

        currentTween = null;
        agent.fsm.ChangeState<EnemyStateWander>();
    }
}