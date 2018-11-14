using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Spinner : MonoBehaviour
{
    [SerializeField] Vector3 rotationPerSecond;

    void Start()
    {
        transform
            .DOLocalRotate(rotationPerSecond, 1f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetRelative()
            .SetLoops(-1, LoopType.Incremental);
    }
}
