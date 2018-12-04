using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : PersistentSingleton<LevelManager>
{
    [Header("Levels")]
    [SerializeField] string mainMenuSceneName  = "MainMenu";
    [SerializeField] string tutorialSceneName  = "Tutorial";
    [SerializeField] string mainLevelSceneName = "Main";

    [Header("Loading screen")]
    [SerializeField] Canvas loadingScreenCanvas;
    [SerializeField] Image loadingProgressBar;

    private AsyncOperation currentLevelLoadingOperation;

    protected override void Awake()
    {
        base.Awake();
        
        if (loadingScreenCanvas && currentLevelLoadingOperation == null) loadingScreenCanvas.gameObject.SetActive(false);

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
        LoadLevel(SceneManager.GetActiveScene().name);
    }
    
    public void LoadLevel(string levelSceneName, bool pauseTimeWhileLoading = true)
    {
        Scene currentScene = SceneManager.GetActiveScene();
        didRestartLevel = currentScene.IsValid() && currentScene.name == levelSceneName;
        
        if (loadingScreenCanvas) loadingScreenCanvas.gameObject.SetActive(true);
        if (pauseTimeWhileLoading) Time.timeScale = 0f;

        StartCoroutine(UpdateProgressBarCoroutine());
        
        currentLevelLoadingOperation = SceneManager.LoadSceneAsync(levelSceneName);
        StartCoroutine(UpdateProgressBarCoroutine());
        currentLevelLoadingOperation.completed += ao =>
        {
            Time.timeScale = 1f;
            if (loadingScreenCanvas) loadingScreenCanvas.gameObject.SetActive(false);

            currentLevelLoadingOperation = null;
            StopAllCoroutines();
                        
            new OnLevelStarted()
                .SetDeliveryType(MessageDeliveryType.Immediate)
                .PostEvent();
        };
    }

    private IEnumerator UpdateProgressBarCoroutine()
    {
        while (currentLevelLoadingOperation != null)
        {
            if (loadingProgressBar) loadingProgressBar.fillAmount = currentLevelLoadingOperation.progress;
            
            yield return null;
        }

        if (loadingProgressBar) loadingProgressBar.fillAmount = 0f;
    }
}