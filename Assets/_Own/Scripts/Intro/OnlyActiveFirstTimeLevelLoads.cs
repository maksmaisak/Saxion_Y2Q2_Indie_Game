using UnityEngine;

public class OnlyActiveFirstTimeLevelLoads : MonoBehaviour
{
    void Awake()
    {
        if (LevelManager.exists && LevelManager.instance.didRestartLevel)
        {
            gameObject.SetActive(false);
        }
    }
}