using System;
using UnityEngine;
using UnityEngine.Assertions;

/// A set of functions menus are most likely to use.
public class MenuButtons : MonoBehaviour
{
	public void StartTutorial() => LevelManager.instance.StartTutorial();
	public void StartMainLevel() => LevelManager.instance.StartMainLevel();
	public void QuitToMainMenu() => LevelManager.instance.GoToMainMenu();
	public void RestartCurrentLevel() => LevelManager.instance.RestartCurrentLevel();
	public void QuitToDesktop() => Quit.ToDesktop();
}
