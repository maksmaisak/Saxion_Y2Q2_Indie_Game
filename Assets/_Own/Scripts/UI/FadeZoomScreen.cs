using System;
using UnityEngine;

public class FadeZoomScreen : TransitionableScreen
{
	[SerializeField] FadeZoom fadeZoom;

	protected override void OnStartUnselected()
	{
		canvasGroup.alpha = 0f;
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;
	}
	
	protected override void OnTransitionIn()
	{
		fadeZoom.FadeIn(canvasGroup, transform);
		//SelectFirstButton();
	}

	protected override void OnTransitionOut()
	{
		fadeZoom.FadeOut(canvasGroup, transform);
	}
}
