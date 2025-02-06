using System;
using System.Collections.Generic;
using Lineri.ESS.Core.Interfaces;


namespace Lineri.ESS.Core;

public interface IEazySoundManager<TAudio, TSource> : IDisposable
where TAudio : IAudio
where TSource : IAudioSource
{
    
    bool CanPlay { get; }

    /// <summary>
    /// Global volume ranging from 0 to 1
    /// </summary>
    float GlobalVolume { get; set; }

    /// <summary>
    /// Global music volume ranging from 0 to 1
    /// </summary>
    float GlobalMusicVolume { get; set; }

    /// <summary>
    /// Global sounds volume ranging from 0 to 1
    /// </summary>
    float GlobalSoundVolume { get; set; }

    /// <summary>
    /// Global UI sounds volume ranging from 0 to 1
    /// </summary>
    float GlobalUISoundVolume { get; set; }
    
    
    /// <summary>
    /// Returns the music Audio that has as its id the audioID if one is found, returns null if no such Audio is found
    /// </summary>
    /// <param name="audioID">The id of the music Audio to be returned</param>
    /// <returns>Music Audio that has as its id the audioID if one is found, null if no such Audio is found</returns>
    TAudio GetMusicAudio(int audioID);

    /// <summary>
    /// Returns the all occurrence of music Audio that plays the given audioClip. Returns null if no such Audio is found
    /// </summary>
    /// <param name="audioClip">The audio clip of the music Audio to be retrieved</param>
    /// <returns>All occurrence of music Audio that has as plays the audioClip, null if no such Audio is found</returns>
    List<TAudio> GetMusicAudios(IAudioClip audioClip);

    /// <summary>
    /// Returns the sound fx Audio that has as its id the audioID if one is found, returns null if no such Audio is found
    /// </summary>
    /// <param name="audioID">The id of the sound fx Audio to be returned</param>
    /// <returns>Sound fx Audio that has as its id the audioID if one is found, null if no such Audio is found</returns>
    TAudio GetSoundAudio(int audioID);

    /// <summary>
    /// Returns the all occurrence of sound Audio that plays the given audioClip. Returns null if no such Audio is found
    /// </summary>
    /// <param name="audioClip">The audio clip of the sound Audio to be retrieved</param>
    /// <returns>All occurrence of sound Audio that has as plays the audioClip, null if no such Audio is found</returns>
    List<TAudio> GetSoundAudios(IAudioClip audioClip);

    /// <summary>
    /// Returns the UI sound fx Audio that has as its id the audioID if one is found, returns null if no such Audio is found
    /// </summary>
    /// <param name="audioID">The id of the UI sound fx Audio to be returned</param>
    /// <returns>UI sound fx Audio that has as its id the audioID if one is found, null if no such Audio is found</returns>
    TAudio GetUISoundAudio(int audioID);

    /// <summary>
    /// Returns the all occurrence of UI sound Audio that plays the given audioClip. Returns null if no such Audio is found
    /// </summary>
    /// <param name="audioClip">The audio clip of the UI sound Audio to be retrieved</param>
    /// <returns>All occurrence of UI sound Audio that has as plays the audioClip, null if no such Audio is found</returns>
    List<TAudio> GetUISoundAudios(IAudioClip audioClip);

    TAudio PlaySound(IAudioClip clip, float volume = 1f, bool loop = false, bool persist = false, 
        float fadeInSeconds = 0f, float fadeOutSeconds = 0f);

    TAudio PlayUISound(IAudioClip clip, float volume = 1f, bool loop = false, bool persist = false, 
        float fadeInSeconds = 0f, float fadeOutSeconds = 0f);

    TAudio PrepareSound(IAudioClip clip, float volume = 1f, bool loop = false, bool persist = false, 
        float fadeInSeconds = 0f, float fadeOutSeconds = 0f);

    TAudio PrepareMusic(IAudioClip clip, float volume = 1f, bool loop = false, bool persist = false, 
        float fadeInSeconds = 0f, float fadeOutSeconds = 0f);

    TAudio PrepareUISound(IAudioClip clip, float volume = 1f, bool loop = false, bool persist = false, 
        float fadeInSeconds = 0f, float fadeOutSeconds = 0f);

    void Update();
    TAudio GetAudio(AudioType audioType, int audioID);
    List<TAudio> GetAudios(AudioType audioType, IAudioClip audioClip);

    TAudio PlayAudio(AudioType audioType, IAudioClip clip, float volume = 1f, bool loop = false, bool persist = false, 
        float fadeInSeconds = 0f, float fadeOutSeconds = 0f);

    TAudio PrepareAudio(AudioType audioType, IAudioClip clip, float volume = 1f, bool loop = false, bool persist = false, 
        float fadeInSeconds = 0f, float fadeOutSeconds = 0f);

    void StopAll();
    void StopAudio(AudioType audioType, float fadeOutSeconds = -1f);
    void PauseAudio(AudioType audioType);
    void PauseAllAudio();
    void UnPauseAudio(AudioType audioType);
    void UnPauseAllAudio();
}