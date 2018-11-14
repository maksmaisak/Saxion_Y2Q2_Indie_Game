using DG.Tweening;
using UnityEngine;

public class Flash : MonoBehaviour
{
    [SerializeField] Vector3 punchScale = Vector3.one * 1.5f;
    [SerializeField] float punchDuration = 0.4f;
    [SerializeField] float punchInterval = 0.4f;
    [SerializeField] int vibrato = 10;
    [SerializeField] float elasticity = 1f;
     
    void Start()
    {
        DOTween
            .Sequence()
            .Append(transform.DOPunchScale(punchScale, punchDuration, vibrato, elasticity))
            .AppendInterval(punchInterval)
            .SetLoops(-1);
    }
}
