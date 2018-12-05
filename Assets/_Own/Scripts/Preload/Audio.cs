using System;
using System.Linq;
using System.Net.Configuration;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;

public class Audio : PersistentSingleton<Audio>
{
    [Header("Soundtrack")]
    [SerializeField] AudioSource soundtrackAudioSource;
    [SerializeField] AudioSource combatSoundtrackAudioSource;
    [SerializeField] AudioClip soundtrack;
    [SerializeField] AudioClip combatSoundTrack;
    [SerializeField] float pitchChangeDuration = 0.5f;
    [SerializeField] float volumeChangeDuration = 0.1f;
    [SerializeField] float combatSoundTrackMaxVolume = 0.6f;
    [SerializeField] float combatFadeInDuration = 0.8f;
    [SerializeField] float combatFadeOutDuration = 1.5f;
    
    private float soundtrackDefaultPitch;
    private float soundtrackDefaultVolume;

    private Tween soundtrackPitchTween;
    private Tween soundtrackVolumeTween;

    private bool isCombatSoundFadingIn = false;
    private bool isCombatSoundFadingOut = false;
    
    private AIManager aiManager;
    
    private void Start()
    {
        aiManager = AIManager.instance;
    }
    
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

        combatSoundtrackAudioSource.clip     = combatSoundTrack;
        combatSoundtrackAudioSource.loop     = true;
        combatSoundtrackAudioSource.Play();
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

    private void Update()
    {
        UpdateCombatSound();
    }

    void UpdateCombatSound()
    {
        if (isCombatSoundFadingIn || isCombatSoundFadingOut)
            return;
        
        if (aiManager.GetAllAgentsInCombat().Count == 0)
        {
            if (Math.Abs(combatSoundtrackAudioSource.volume) < 0.01f)
                return;
            
            isCombatSoundFadingOut = true;
            combatSoundtrackAudioSource
                .DOFade(0, combatFadeOutDuration)
                .OnComplete(() => isCombatSoundFadingOut = false);
        }
        else if (aiManager.GetAllAgentsInCombat().Count > 0)
        {
            if (Math.Abs(combatSoundtrackAudioSource.volume) >= combatSoundTrackMaxVolume)
                return;
            
            isCombatSoundFadingIn = true;
            combatSoundtrackAudioSource
                .DOFade(combatSoundTrackMaxVolume, combatFadeInDuration)
                .OnComplete(() => isCombatSoundFadingIn = false);
        }
    }
}