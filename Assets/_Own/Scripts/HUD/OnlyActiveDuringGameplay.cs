using System;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class OnlyActiveDuringGameplay : MyBehaviour,
	IEventReceiver<OnPlayerDied>,
	IEventReceiver<OnLevelCompleted>
{
	[SerializeField] float fadeOutDuration = 1f;
	
	private CanvasGroup canvasGroup;

	public void On(OnPlayerDied message) => OnGameplayEnded();
	public void On(OnLevelCompleted message) => OnGameplayEnded();
	
	private void OnGameplayEnded()
	{
		canvasGroup = canvasGroup ? canvasGroup : GetComponent<CanvasGroup>();
		
		canvasGroup.DOKill();
		canvasGroup.interactable = canvasGroup.blocksRaycasts = false;
		canvasGroup.DOFade(0f, duration: fadeOutDuration);
	}
}
