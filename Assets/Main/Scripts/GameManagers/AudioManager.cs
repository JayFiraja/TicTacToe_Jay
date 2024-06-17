using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


/// <summary>
/// Manages the different audio calls
/// </summary>
public class AudioManager : SubManager
{
    #region Members
    [SerializeField]
    private AudioMixer audioMixer;

    [SerializeField]
    public AudioSource voiceS;

    [SerializeField]
    public AudioSource UISFXSource;

    [SerializeField]
    private List<AudioSource> positionSources = null;
    private int lastPosPlayed;

    private AudioMixerGroup currentGroup;
    
    #endregion
    
    public void PlayVoiceSFX(AudioClip[] fx)
    {
        if (!TryGetRandomSoundEffect(fx, out AudioClip availableClip))
        {
            return;
        }
        
        voiceS.PlayOneShot(availableClip, voiceS.volume); 
    }
    
    #region UI SFX
    
    public void PlayUISFX(AudioClip a)
    {
        if (UISFXSource != null)
        {
            UISFXSource.PlayOneShot(a, UISFXSource.volume); 
        }
    }
    
    #endregion

#region PositionFx

    /// <summary>
    /// Plays a random clip from an array of AudioClips at a location.
    /// </summary>
    /// <param name="fx"></param>
    /// <param name="pos"></param>
    public void PlayClipAt(AudioClip[] fx, Vector3 pos)
    {
        if (!TryGetRandomSoundEffect(fx, out AudioClip availableClip))
        {
            return;
        }

        PlayClipAt(availableClip, pos);
    }

    /// <summary>
    /// Plays a clip at a given world position
    /// </summary>
    public void PlayClipAt(AudioClip sfx, Vector3 pos)
    {
        if (positionSources == null)
        {
            return;
        }
        
        if (lastPosPlayed >= positionSources.Count)
        {
            lastPosPlayed = 0;
        }
    
        if (lastPosPlayed < positionSources.Count)
        {
            positionSources[lastPosPlayed].transform.position = pos;
            positionSources[lastPosPlayed].PlayOneShot(sfx);
            lastPosPlayed++;
        }
    }

    private bool TryGetRandomSoundEffect(AudioClip[] clips, out AudioClip availableClip)
    {
        availableClip = null;
        if (clips == null || clips.Length <= 0)
        {
            return false;
        }
        
        int randomClip = Random.Range(0, clips.Length);
        availableClip = clips[randomClip];

        return availableClip != null;
    }

#endregion

}

