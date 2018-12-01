using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

public class LevelExit : MonoBehaviour
{
    // TODO have this keep track of the stats.

    private bool didTrigger;
    
    private void OnTriggerEnter(Collider other)
    {
        if (didTrigger) return;
        // TODO Use something that doesn't depend on this script to identify the player.
        if (!other.gameObject.GetComponentInParent<ThirdPersonCharacter>()) return;
        didTrigger = true;
        
        // TEMP hardcoded values
        new LevelFinished(100, 15, 7).PostEvent();
    }
}