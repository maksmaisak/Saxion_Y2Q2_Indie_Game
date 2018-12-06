using UnityEngine;
using UnityEngine.Events;

public class OnDestroyListener : MonoBehaviour
{
    [SerializeField] UnityEvent OnDestroyed;
    void OnDestroy() => OnDestroyed.Invoke();
}