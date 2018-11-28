using System.Net.Configuration;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;

public class Audio : Singleton<Audio>
{
    [Header("Soundtrack")]
    [SerializeField] AudioSource soundtrackAudioSource;
    [SerializeField] AudioClip soundtrack;
    [SerializeField] float pitchChangeDuration = 0.5f;
    [SerializeField] float volumeChangeDuration = 0.1f;
    
    private float soundtrackDefaultPitch;
    private float soundtrackDefaultVolume;

    private Tween soundtrackPitchTween;
    private Tween soundtrackVolumeTween;

    protected override void Awake()
    {
        base.Awake();
        
        if (!soundtrack) return;
        if (!soundtrackAudioSource) return;

        soundtrackDefaultPitch = soundtrackAudioSource.pitch;
        soundtrackDefaultVolume = soundtrackAudioSource.volume;

        soundtrackAudioSource.clip = soundtrack;
        soundtrackAudioSource.loop = true;
        soundtrackAudioSource.Play();
    }
    
    public void SetMusicVolume(float normalizedTargetVolume, bool immediate = false)
    {
        soundtrackVolumeTween?.Kill();
        float targetVolume = normalizedTargetVolume * soundtrackDefaultVolume;
        if (immediate)
        {
            soundtrackAudioSource.volume = targetVolume;
            return;
        }
        
        soundtrackVolumeTween = soundtrackAudioSource
            .DOFade(normalizedTargetVolume * soundtrackDefaultVolume, volumeChangeDuration);
    }
}