using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

public class LevelExit : MyBehaviour
{
    private bool didTrigger;
    
    private void OnTriggerEnter(Collider other)
    {
        if (didTrigger) return;
        // TODO Use something that doesn't depend on this script to identify the player.
        if (!other.gameObject.GetComponentInParent<ThirdPersonCharacter>()) return;
        didTrigger = true;
        
        // TEMP hardcoded values
        new LevelFinished().PostEvent();
    }
}