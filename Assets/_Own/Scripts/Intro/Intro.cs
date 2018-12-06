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

	IEnumerator Start()
	{
		TimeHelper.timeScale = 0f;
		
		string text = textMesh.text;
		textMesh.text = String.Empty;
		yield return new WaitForSeconds(initialDelay);

		Assert.IsNotNull(textMesh);

		var seq = DOTween.Sequence().SetUpdate(isIndependentUpdate: true);

		seq.Append(textMesh.DOText(text, timePerLetter * text.Length));
		seq.AppendInterval(fadeoutDelay);
		seq.Append(canvasGroup.DOFade(0f, fadeoutTime));

		yield return seq.WaitForCompletion();
		TimeHelper.timeScale = 1f;
	}
}
