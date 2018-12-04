using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : PersistentSingleton<LevelManager>
{
    [SerializeField] string mainMenuSceneName  = "MainMenu";
    [SerializeField] string tutorialSceneName  = "Tutorial";
    [SerializeField] string mainLevelSceneName = "Main";

    protected override void Awake()
    {
        base.Awake();

        if (SceneManager.sceneCount == 1 && SceneManager.GetActiveScene().name == "__preload")
        {
            GoToMainMenu();
        }
    }

    /// <summary>
    /// Use this to determine if you need to play an intro cutscene.
    /// </summary>
    public bool didRestartLevel { get; private set; }

    public void GoToMainMenu()
    {
        didRestartLevel = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void StartTutorial() => LoadLevel(tutorialSceneName);
    public void StartMainLevel() => LoadLevel(mainLevelSceneName);
    
    public void RestartCurrentLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        new OnLevelStarted()
            .SetDeliveryType(MessageDeliveryType.Immediate)
            .PostEvent();
    }

    public void LoadLevel(string levelName)
    {
        Scene currentScene = SceneManager.GetActiveScene();
        didRestartLevel = currentScene.IsValid() && currentScene.name == levelName;
        
        SceneManager.LoadScene(levelName);
    }
}