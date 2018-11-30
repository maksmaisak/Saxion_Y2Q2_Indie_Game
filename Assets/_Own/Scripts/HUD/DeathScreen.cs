using System;
using DG.Tweening;
using UnityEngine;

public class DeathScreen : MyBehaviour, IEventReceiver<PlayerDied>
{
	[SerializeField] CanvasGroup main;
	[SerializeField] float mainFadeInDelay = 2f;
	[SerializeField] float mainFadeInTime = 2f;
	[Space]
	[SerializeField] CanvasGroup buttons;
	[SerializeField] private float buttonFadeInDelay = 5f;
	[SerializeField] private float buttonsFadeInTime = 2f;
	
	public void On(PlayerDied message)
	{
		foreach (Transform child in transform)
		{
			child.gameObject.SetActive(true);
		}

		if (main)
		{
			main.alpha = 0f;
			main
				.DOFade(1f, mainFadeInTime)
				.SetDelay(mainFadeInDelay);
		}

		if (buttons)
		{
			buttons.alpha = 0f;
			buttons.interactable = false;
			buttons.blocksRaycasts = false;

			buttons
				.DOFade(1f, buttonsFadeInTime)
				.SetDelay(buttonFadeInDelay)
				.SetEase(Ease.InExpo)
				.onComplete += () => 
			{
				buttons.interactable = true;
				buttons.blocksRaycasts = true;
			};
		}
 	}
}
