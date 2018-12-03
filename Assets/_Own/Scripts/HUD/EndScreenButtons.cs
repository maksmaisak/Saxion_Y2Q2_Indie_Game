using UnityEngine;

public class EndScreenButtons : MonoBehaviour
{
    public void RestartLevel() => LevelManager.instance.RestartCurrentLevel();
    public void GoToMainMenu() => LevelManager.instance.GoToMainMenu();
    public void QuitToDesktop() => Quit.ToDesktop();
}