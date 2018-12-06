using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Audio : PersistentSingleton<Audio>
{
    [Header("Soundtrack")]
    [SerializeField] float volumeChangeDuration = 0.1f;
    [SerializeField] AudioClip soundtrack;
    [SerializeField] AudioSource soundtrackAudioSource;

    [Header("Main Menu Sound Settings")]
    [SerializeField] float mainMenuFadeInDuration = 1.0f;
    [SerializeField] float mainMenuFadeOutDuration = 2.0f;
    [SerializeField] float mainMenuSoundTrackMaxVolume = 0.6f;
    [SerializeField] AudioSource mainMenuSoundtrackAudioSource;

    [Header("Combat Sound Settings")]
    [SerializeField] AudioClip combatSoundTrack;
    [SerializeField] AudioSource combatSoundtrackAudioSource;
    [SerializeField] float combatFadeInDuration = 0.8f;
    [SerializeField] float combatFadeOutDuration = 1.5f;
    [SerializeField] float combatSoundTrackMaxVolume = 0.6f;

    private float soundtrackDefaultVolume;

    private Tween soundtrackPitchTween;
    private Tween soundtrackVolumeTween;

    private bool isCombatSoundFadingIn = false;
    private bool isCombatSoundFadingOut = false;

    private AIManager aiManager;

    private void Start()
    {
        aiManager = AIManager.instance;
       SceneManager.activeSceneChanged += OnActiveSceneChange;
    }
    
    protected override void Awake()
    {
        base.Awake();
        
        if (!soundtrack) return;
        if (!soundtrackAudioSource) return;
        
        soundtrackDefaultVolume = soundtrackAudioSource.volume;

        soundtrackAudioSource.clip = soundtrack;
        soundtrackAudioSource.loop = true;
        soundtrackAudioSource.Play();

        combatSoundtrackAudioSource.clip = combatSoundTrack;
        combatSoundtrackAudioSource.loop = true;
        combatSoundtrackAudioSource.Play();

        mainMenuSoundtrackAudioSource.volume = 0f;
        mainMenuSoundtrackAudioSource.loop   = true;

        this.DoNextFrame(PlayMainMenuSoundTrackIfAtMainMenu);
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
            .DOFade(normalizedTargetVolume * soundtrackDefaultVolume, volumeChangeDuration)
            .SetUpdate(isIndependentUpdate: true);
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

    void OnActiveSceneChange(Scene currentScene, Scene nextScene) => this.DoNextFrame(PlayMainMenuSoundTrackIfAtMainMenu);

    private void PlayMainMenuSoundTrackIfAtMainMenu()
    {
        if (LevelManager.instance.isAtMainMenu)
        {
            SetMusicVolume(0f);
            
            mainMenuSoundtrackAudioSource.Play();
            mainMenuSoundtrackAudioSource.DOKill();
            mainMenuSoundtrackAudioSource
                .DOFade(mainMenuSoundTrackMaxVolume, mainMenuFadeInDuration)
                .SetUpdate(isIndependentUpdate: true);
        }
        else
        {
            SetMusicVolume(1f);
            
            mainMenuSoundtrackAudioSource.DOKill();
            mainMenuSoundtrackAudioSource
                .DOFade(0f, mainMenuFadeOutDuration)
                .SetUpdate(isIndependentUpdate: true)
                .OnComplete(mainMenuSoundtrackAudioSource.Stop);
        }
    }
}