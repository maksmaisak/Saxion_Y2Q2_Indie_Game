using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : PersistentSingleton<LevelManager>
{   
    public void RestartCurrentLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        new OnLevelStarted()
            .SetDeliveryType(MessageDeliveryType.Immediate)
            .PostEvent();
    }

    public void GoToMainMenu()
    {
        throw new System.NotImplementedException();
    }
}