using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : Singleton<LevelManager>
{   
    public void RestartCurrentLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        new LevelStarted()
            .SetDeliveryType(MessageDeliveryType.Immediate)
            .PostEvent();
    }

    public void GoToMainMenu()
    {
        throw new System.NotImplementedException();
    }
}