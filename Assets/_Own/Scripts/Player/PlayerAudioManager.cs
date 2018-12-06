using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerAudioManager : MonoBehaviour
{
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
