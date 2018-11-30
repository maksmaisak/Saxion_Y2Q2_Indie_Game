using System;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class OnlyActiveDuringGameplay : MyBehaviour,
	IEventReceiver<PlayerDied>
{
	[SerializeField] float fadeOutDuration = 1f;
	
	private CanvasGroup canvasGroup;

	void Start()
	{
		canvasGroup = GetComponent<CanvasGroup>();
	}

	public void On(PlayerDied message) => OnGameplayEnded();
	
	private void OnGameplayEnded()
	{
		canvasGroup.DOKill();
		canvasGroup.interactable = canvasGroup.blocksRaycasts = false;
		canvasGroup.DOFade(0f, duration: fadeOutDuration);
	}
}
