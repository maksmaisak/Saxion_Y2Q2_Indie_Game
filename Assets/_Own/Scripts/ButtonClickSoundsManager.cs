﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class ButtonClickSoundsManager : MonoBehaviour
{
	[SerializeField] private AudioSource audioSource;
	
	// Use this for initialization
	void Start()
	{
		if (!audioSource) audioSource = GetComponent<AudioSource>();
		Assert.IsNotNull(audioSource);

		SceneHelper.instance.OnActiveSceneChange += AssignSoundToButtons;
		
		AssignSoundToButtons();
	}
	
	void AssignSoundToButtons()
	{
		foreach (Button button in FindObjectsOfType<Button>())
			button.onClick.AddListener(() => audioSource.Play());
	}
}
