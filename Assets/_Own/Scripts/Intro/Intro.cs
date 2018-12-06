using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class Intro : MonoBehaviour
{
	[SerializeField] TMP_Text textMesh;
	[SerializeField] float initialDelay = 2f;
	[SerializeField] float timePerLetter = 0.1f;
	[Space] 
	[SerializeField] CanvasGroup canvasGroup;
	[SerializeField] float fadeoutDelay = 1f;
	[SerializeField] float fadeoutTime = 1f;

	private bool keepTimeScaleZero = true;

	IEnumerator Start()
	{
		if (LevelManager.instance.didRestartLevel)
		{
			Destroy(gameObject);
			yield break;
		}
		
		TimeHelper.timeScale = 0f;
		keepTimeScaleZero = true;
		PauseScreen.canPause = false;
		
		string text = textMesh.text;
		textMesh.text = String.Empty;
		yield return new WaitForSecondsRealtime(initialDelay);

		Assert.IsNotNull(textMesh);

		var seq = DOTween.Sequence().SetUpdate(isIndependentUpdate: true);

		seq.Append(textMesh.DOText(text, timePerLetter * text.Length));
		seq.AppendInterval(fadeoutDelay);
		seq.Append(canvasGroup.DOFade(0f, fadeoutTime));
		
		yield return seq.WaitForCompletion();
		
		keepTimeScaleZero = false;
		TimeHelper.timeScale = 1f;
		PauseScreen.canPause = true;
	}

	private void Update()
	{
		if (keepTimeScaleZero)
		{
			TimeHelper.timeScale = 0f;
		}
	}
}
