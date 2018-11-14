using UnityEngine;
using UnityEngine.SceneManagement;

public class Quitter : MonoBehaviour
{
    public void QuitToDesktop() => Quit.ToDesktop();
    public void QuitToMenu() => SceneManager.LoadScene(SceneNames.mainMenuName);
}