using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/// Destroys the gameobject once all child particle systems and audio sources have finished playing.
public class AutoDestroyEffect : MonoBehaviour
{
    private ParticleSystem[] particleSystems;
    private AudioSource[] audioSources;
    
    IEnumerator Start()
    {
        particleSystems = GetComponentsInChildren<ParticleSystem>();
        audioSources = GetComponentsInChildren<AudioSource>();
        
        float minDuration = Mathf.Max(GetParticlesDuration(), GetAudioDuration());
        if (!float.IsPositiveInfinity(minDuration))
        {
            yield return new WaitForSeconds(minDuration);
        }

        yield return new WaitUntil(() => 
            !particleSystems.Any(ps => ps.IsAlive() && !audioSources.Any(a => a.isPlaying))
        );
        Destroy(gameObject, minDuration);
    }

    private float GetParticlesDuration()
    {
        if (particleSystems.Length == 0) return 0f;
        return particleSystems.Max(ps => GetParticleSystemDuration(ps));
    }

    private float GetAudioDuration()
    {
        if (audioSources.Length == 0) return 0f;
        return audioSources.Max(source => source.clip.length);
    }

    private float GetParticleSystemDuration(ParticleSystem particleSystem)
    {
        float emissionDuration = particleSystem.main.duration;

        // Does not account for other MinMaxCurve modes, but should do for now.
        ParticleSystem.MinMaxCurve startLifetime = particleSystem.main.startLifetime;
        if (startLifetime.mode == ParticleSystemCurveMode.Constant)
        {
            //Debug.LogWarning("AutoDestroyParticleSystem doesn't support non-constant startLifetime settings yet. The particle system might be destroyed earlier than needed.");
        }
        float particleLifetime = startLifetime.constant;

        return emissionDuration + particleLifetime;
    }
}
