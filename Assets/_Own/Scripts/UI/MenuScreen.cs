using System;
using UnityEngine;
using UnityEngine.Assertions;

public class MenuScreen : TransitionableScreen
{
	[SerializeField] FadeZoom fadeZoom;
	
	protected override void OnTransitionIn()
	{
		fadeZoom.FadeIn(canvasGroup, transform);
		SelectFirstButton();
	}

	protected override void OnTransitionOut()
	{
		fadeZoom.FadeOut(canvasGroup, transform);
	}
}
