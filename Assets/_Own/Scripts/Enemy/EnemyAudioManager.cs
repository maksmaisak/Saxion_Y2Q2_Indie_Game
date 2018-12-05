using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public class EnemyAudioManager : MonoBehaviour {

	[SerializeField] AudioClip[] footStepAudioClips;
	[SerializeField] AudioSource footStepAudioSource;
	
	void Start()
	{
		Assert.IsNotNull(footStepAudioSource);
		Assert.IsTrue(footStepAudioClips.Length > 0);
	}
    
	// Function called with animation key events
	void PlayFootStepSound()
	{
		footStepAudioSource.clip = footStepAudioClips[Random.Range(0, footStepAudioClips.Length - 1)];
		footStepAudioSource.Play();
	}
}