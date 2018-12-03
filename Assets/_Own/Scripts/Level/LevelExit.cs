using UnityEngine;
using UnityEngine.Events;
using UnityStandardAssets.Characters.ThirdPerson;

public class LevelExit : MyBehaviour
{
    [SerializeField] UnityEvent OnComplete;
    
    private bool didTrigger;
    
    private void OnTriggerEnter(Collider other)
    {
        if (didTrigger) return;
        // TODO Use something that doesn't depend on this script to identify the player.
        if (!other.gameObject.GetComponentInParent<ThirdPersonCharacter>()) return;
        didTrigger = true;
        
        OnComplete.Invoke();
        new OnLevelCompleted().PostEvent();
    }
}