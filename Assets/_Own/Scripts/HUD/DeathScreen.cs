using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DeathScreen : MyBehaviour, IEventReceiver<PlayerDied>
{
	[SerializeField] CanvasGroup main;
	[SerializeField] float mainFadeInDelay = 2f;
	[SerializeField] float mainFadeInTime = 2f;
	[Space]
	[SerializeField] CanvasGroup buttons;
	[SerializeField] private float buttonFadeInDelay = 5f;
	[SerializeField] private float buttonsFadeInTime = 2f;

	void Start()
	{
		foreach (Transform child in transform)
		{
			child.gameObject.SetActive(false);
		}
	}

	public void On(PlayerDied message)
	{
		if (!isActiveAndEnabled) return;
				
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
			main.interactable = main.blocksRaycasts = true;
		}

		if (buttons)
		{
			buttons.alpha = 0f;
			buttons.interactable = false;
			buttons.blocksRaycasts = false;

			DOTween.Sequence()
				.AppendInterval(buttonFadeInDelay)
				.AppendCallback(() =>
				{
					CursorHelper.SetLock(false);
					buttons.interactable = true;
					buttons.blocksRaycasts = true;
					buttons.GetComponentInChildren<Button>()?.Select();
				})
				.Append(
					buttons
					.DOFade(1f, buttonsFadeInTime)
					.SetEase(Ease.InExpo)
				);
		}
 	}
}
