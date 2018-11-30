using UnityEngine;

public class LevelExit : MonoBehaviour
{
    // TODO have this keep track of the stats.

    [SerializeField] string playerTag = "Player";

    private bool didTrigger;
    
    private void OnTriggerEnter(Collider other)
    {
        if (!didTrigger) return;
        if (!other.gameObject.CompareTag(playerTag)) return;

        didTrigger = true;
        new LevelFinished().PostEvent();
    }
}