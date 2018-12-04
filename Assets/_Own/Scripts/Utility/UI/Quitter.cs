using UnityEngine;
using UnityEngine.SceneManagement;

public class Quitter : MonoBehaviour
{
    public void QuitToDesktop() => Quit.ToDesktop();
    public void QuitToMenu() => LevelManager.instance.GoToMainMenu();
}