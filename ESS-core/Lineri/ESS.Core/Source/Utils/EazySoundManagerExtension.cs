using System.Collections.Generic;
using Lineri.ESS.Core.Interfaces;


namespace Lineri.ESS.Core;

public partial class EazySoundManager
{

    #region GetAudio
    /// <summary>
    /// Returns the music Audio that has as its id the audioID if one is found, returns null if no such Audio is found
    /// </summary>
    /// <param name="audioID">The id of the music Audio to be returned</param>
    /// <returns>Music Audio that has as its id the audioID if one is found, null if no such Audio is found</returns>
    public static Audio GetMusicAudio(int audioID)
    {
        return GetAudio(AudioType.Music, audioID);
    }

    /// <summary>
    /// Returns the all occurrence of music Audio that plays the given audioClip. Returns null if no such Audio is found
    /// </summary>
    /// <param name="audioClip">The audio clip of the music Audio to be retrieved</param>
    /// <returns>All occurrence of music Audio that has as plays the audioClip, null if no such Audio is found</returns>
    public static List<Audio> GetMusicAudio(IAudioClip audioClip)
    {
        return GetAudios(AudioType.Music, audioClip);
    }

    /// <summary>
    /// Returns the sound fx Audio that has as its id the audioID if one is found, returns null if no such Audio is found
    /// </summary>
    /// <param name="audioID">The id of the sound fx Audio to be returned</param>
    /// <returns>Sound fx Audio that has as its id the audioID if one is found, null if no such Audio is found</returns>
    public static Audio GetSoundAudio(int audioID)
    {
        return GetAudio(AudioType.Sound, audioID);
    }

    /// <summary>
    /// Returns the all occurrence of sound Audio that plays the given audioClip. Returns null if no such Audio is found
    /// </summary>
    /// <param name="audioClip">The audio clip of the sound Audio to be retrieved</param>
    /// <returns>All occurrence of sound Audio that has as plays the audioClip, null if no such Audio is found</returns>
    public static List<Audio> GetSoundAudio(IAudioClip audioClip)
    {
        return GetAudios(AudioType.Sound, audioClip);
    }

    /// <summary>
    /// Returns the UI sound fx Audio that has as its id the audioID if one is found, returns null if no such Audio is found
    /// </summary>
    /// <param name="audioID">The id of the UI sound fx Audio to be returned</param>
    /// <returns>UI sound fx Audio that has as its id the audioID if one is found, null if no such Audio is found</returns>
    public static Audio GetUISoundAudio(int audioID)
    {
        return GetAudio(AudioType.UISound, audioID);
    }

    /// <summary>
    /// Returns the all occurrence of UI sound Audio that plays the given audioClip. Returns null if no such Audio is found
    /// </summary>
    /// <param name="audioClip">The audio clip of the UI sound Audio to be retrieved</param>
    /// <returns>All occurrence of UI sound Audio that has as plays the audioClip, null if no such Audio is found</returns>
    public static List<Audio> GetUISoundAudio(IAudioClip audioClip)
    {
        return GetAudios(AudioType.UISound, audioClip);
    }
    #endregion

    #region Play Audio
    public static Audio PlaySound(IAudioClip clip, float volume = 1f, bool loop = false, bool persist = false, 
        float fadeInSeconds = 0f, float fadeOutSeconds = 0f) => 
        PlayAudio(AudioType.Sound, clip, volume, loop, persist, fadeInSeconds, fadeOutSeconds);
    public static Audio PlayMusic(IAudioClip clip, float volume = 1f, bool loop = false, bool persist = false, 
        float fadeInSeconds = 0f, float fadeOutSeconds = 0f, float currentMusicFadeOutSeconds = 0f) => 
        PlayAudio(AudioType.Music, clip, volume, loop, persist, fadeInSeconds, fadeOutSeconds, currentMusicFadeOutSeconds);
    public static Audio PlayUISound(IAudioClip clip, float volume = 1f, bool loop = false, bool persist = false, 
        float fadeInSeconds = 0f, float fadeOutSeconds = 0f) => 
        PlayAudio(AudioType.UISound, clip, volume, loop, persist, fadeInSeconds, fadeOutSeconds);
    #endregion

    #region Prepare Audio
    public static Audio PrepareSound(IAudioClip clip, float volume = 1f, bool loop = false, bool persist = false, 
        float fadeInSeconds = 0f, float fadeOutSeconds = 0f) => 
        PrepareAudio(AudioType.Sound, clip, volume, loop, persist, fadeInSeconds, fadeOutSeconds);
        
    public static Audio PrepareMusic(IAudioClip clip, float volume = 1f, bool loop = false, bool persist = false, 
        float fadeInSeconds = 0f, float fadeOutSeconds = 0f) => 
        PrepareAudio(AudioType.Music, clip, volume, loop, persist, fadeInSeconds, fadeOutSeconds);
        
    public static Audio PrepareUISound(IAudioClip clip, float volume = 1f, bool loop = false, bool persist = false, 
        float fadeInSeconds = 0f, float fadeOutSeconds = 0f) => 
        PrepareAudio(AudioType.UISound, clip, volume, loop, persist, fadeInSeconds, fadeOutSeconds);
    #endregion

}
